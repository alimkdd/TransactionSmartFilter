using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TransactSmartFilter.Application.Dtos.Requests;
using TransactSmartFilter.Application.Dtos.Responses;
using TransactSmartFilter.Application.Interfaces;
using TransactSmartFilter.Application.Services.Utilities;
using TransactSmartFilter.Domain.Enums;
using TransactSmartFilter.Domain.Models;
using TransactSmartFilter.Infrastructure.Context;

namespace TransactSmartFilter.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _dbContext;
    private readonly IUserTierService _userTierService;
    private readonly ITransactionQueryBuilder _queryBuilder;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDistributedCache _cache;

    public TransactionService(
        AppDbContext dbContext,
        IUserTierService userTierService,
        ITransactionQueryBuilder queryBuilder,
        IPublishEndpoint publishEndpoint,
        IDistributedCache cache)
    {
        _dbContext = dbContext;
        _userTierService = userTierService;
        _queryBuilder = queryBuilder;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
    }

    public async Task<TransactionSearchResponse> Search(TransactionSearchRequest request)
    {
        try
        {
            var stopwatch = ValueStopwatch.StartNew();

            var isAdmin = await _userTierService.IsAdmin(request.UserId);
            var userTier = await _userTierService.GetUserTier(request.UserId);
            var sharedAccounts = await _userTierService.GetSharedAccountIds(request.UserId);

            // Validate and normalize request
            request = ValidateRequest(request);
            request = NormalizeFilters(request, userTier);

            // Async search for > 6 months
            if ((request.ToDate - request.FromDate)?.TotalDays > 180)
            {
                return new TransactionSearchResponse(
                    Results: new List<TransactionResponse>(),
                    TotalCount: 0,
                    Page: 0,
                    PageSize: 0,
                    TotalPages: 0,
                    HasMore: false,
                    Metadata: new SearchInfoResponse(
                        QueryTime: TimeSpan.Zero,
                        AppliedFilters: "Long-running search queued",
                        JobId: await EnqueueAsyncSearch(request)
                    )
                );
            }

            // Cache By Redis 
            string cacheKey = $"user:{request.UserId}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<TransactionSearchResponse>(cached);
            }

            // Build query (optimized order)
            var queryBuilder = _queryBuilder
                .ForUser(request.UserId, isAdmin, sharedAccounts)
                .WithDateRange(request.FromDate, request.ToDate)
                .WithAmountRange(request.MinAmount, request.MaxAmount, request.ExactAmount)
                .WithTypes(request.TransactionTypeIds)
                .WithStatuses(request.TransactionStatusIds)
                .WithPaymentMethods(request.PaymentMethodIds)
                .WithTags(request.TagIds)
                .SearchRecipient(request.Recipient)
                .SearchDescription(request.Description, request.FromDate);

            // Total count for pagination
            var totalCount = await queryBuilder.Build().CountAsync();

            // Pagination and sorting
            var pagedQuery = queryBuilder
                .SortBy(request.SortBy, request.SortDirection)
                .Paginate(request.Page, request.PageSize)
                .Build();

            var transactions = await pagedQuery
                .Include(t => t.TransactionTags)
                    .ThenInclude(tt => tt.Tag)
                .ToListAsync();

            var transactionRecords = transactions.Adapt<List<TransactionResponse>>();

            var elapsed = stopwatch.GetElapsedTime();

            var response = new TransactionSearchResponse(
                Results: transactionRecords,
                TotalCount: totalCount,
                Page: request.Page,
                PageSize: request.PageSize,
                TotalPages: (int)Math.Ceiling(totalCount / (double)request.PageSize),
                HasMore: request.Page * request.PageSize < totalCount,
                Metadata: new SearchInfoResponse(
                    QueryTime: elapsed,
                    AppliedFilters: BuildAppliedFilters(request),
                    JobId: Guid.Empty
                )
            );

            // Cache response in Redis
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(response),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1)
                });

            return response;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<TransactionSearchResponse> Job(Guid jobId)
    {
        try
        {
            var job = await _dbContext.TransactionSearchJobs.FindAsync(jobId);

            if (job == null)
                throw new KeyNotFoundException($"Job {jobId} not found");

            if (job.Status != Status.Completed.ToString())
            {
                return new TransactionSearchResponse(
                    Results: new List<TransactionResponse>(),
                    TotalCount: 0,
                    Page: 0,
                    PageSize: 0,
                    TotalPages: 0,
                    HasMore: false,
                    Metadata: new SearchInfoResponse(
                        QueryTime: TimeSpan.Zero,
                        AppliedFilters: $"Job {jobId} not completed",
                        JobId: jobId
                    )
                );
            }

            return JsonSerializer.Deserialize<TransactionSearchResponse>(job.ResultJson);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private TransactionSearchRequest ValidateRequest(TransactionSearchRequest request)
    {
        try
        {
            int page = request.Page < 1 ? 1 : request.Page;
            int pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);
            var sortBy = string.IsNullOrEmpty(request.SortBy) || !new[] { "date", "amount", "status" }.Contains(request.SortBy.ToLower())
                ? "date"
                : request.SortBy;
            var sortDir = string.IsNullOrEmpty(request.SortDirection) || !new[] { "asc", "desc" }.Contains(request.SortDirection.ToLower())
                ? "desc"
                : request.SortDirection;

            return request with { Page = page, PageSize = pageSize, SortBy = sortBy, SortDirection = sortDir };
        }
        catch (Exception)
        {
            throw;
        }
    }

    private TransactionSearchRequest NormalizeFilters(TransactionSearchRequest request, string userTier)
    {
        try
        {
            var now = DateTime.UtcNow;

            // Exact amount takes precedence
            if (request.ExactAmount.HasValue)
                request = request with { MinAmount = null, MaxAmount = null };

            // Predefined date ranges (only if FromDate is not set)
            if (!request.FromDate.HasValue && !string.IsNullOrEmpty(request.PredefinedRange))
            {
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfYear = new DateTime(now.Year, 1, 1);

                request = request.PredefinedRange?.ToLower() switch
                {
                    "today" => request with { FromDate = now.Date, ToDate = now },
                    "yesterday" => request with { FromDate = now.Date.AddDays(-1), ToDate = now.Date.AddTicks(-1) },
                    "last7days" => request with { FromDate = now.AddDays(-7), ToDate = now },
                    "last30days" => request with { FromDate = now.AddDays(-30), ToDate = now },
                    "thismonth" => request with { FromDate = startOfMonth, ToDate = now },
                    "lastmonth" => request with { FromDate = startOfMonth.AddMonths(-1), ToDate = startOfMonth.AddTicks(-1) },
                    "thisyear" => request with { FromDate = startOfYear, ToDate = now },
                    "lastyear" => request with { FromDate = new DateTime(now.Year - 1, 1, 1), ToDate = new DateTime(now.Year - 1, 12, 31, 23, 59, 59, 999) },
                    _ => request
                };
            }

            // Default range if still missing (last 30 days)
            var fromDate = request.FromDate ?? now.AddDays(-30);
            var toDate = request.ToDate ?? now;

            // Swap if reversed
            if (fromDate > toDate)
                (fromDate, toDate) = (toDate, fromDate);

            // Max allowed duration by tier
            var maxDays = userTier switch
            {
                "Regular" => 90,
                "Premium" => 365,
                "Admin" => double.MaxValue,
                _ => 90
            };

            if ((toDate - fromDate).TotalDays > maxDays)
                throw new ArgumentException($"Date range exceeds allowed maximum of {maxDays} days for your user tier.");

            return request with { FromDate = fromDate, ToDate = toDate };
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<Guid> EnqueueAsyncSearch(TransactionSearchRequest request)
    {
        try
        {
            var requestJson = JsonSerializer.Serialize(request);

            // Check if a queued job already exists for the same account and request
            var existingJob = await _dbContext.TransactionSearchJobs
                .FirstOrDefaultAsync(j => j.AccountId == request.UserId
                                       && j.RequestJson == requestJson
                                       && j.Status == Status.Queued.ToString());

            if (existingJob != null)
                return existingJob.Id;

            var job = new TransactionSearchJob
            {
                AccountId = request.UserId,
                RequestJson = requestJson,
                Status = Status.Queued.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.TransactionSearchJobs.Add(job);
            await _dbContext.SaveChangesAsync();

            await _publishEndpoint.Publish(new TransactionSearchJobRequest(job.Id, job.AccountId, job.RequestJson));

            return job.Id;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private string BuildAppliedFilters(TransactionSearchRequest request)
    {
        try
        {
            return $"FromDate={request.FromDate}, ToDate={request.ToDate}," +
                   $" Types={string.Join(",", request.TransactionTypeIds ?? new List<int>())}," +
                   $" AmountRange={request.MinAmount}-{request.MaxAmount}," +
                   $" ExactAmount={request.ExactAmount}";
        }
        catch (Exception)
        {
            throw;
        }
    }
}