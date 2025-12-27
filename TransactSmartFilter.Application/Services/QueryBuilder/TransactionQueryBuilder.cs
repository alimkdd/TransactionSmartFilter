using Microsoft.EntityFrameworkCore;
using TransactSmartFilter.Application.Interfaces;
using TransactSmartFilter.Domain.Models;
using TransactSmartFilter.Infrastructure.Context;

namespace TransactSmartFilter.Application.Services.QueryBuilder;

public class TransactionQueryBuilder : ITransactionQueryBuilder
{
    private IQueryable<Transaction> _query;
    private readonly AppDbContext _context;

    public TransactionQueryBuilder(AppDbContext context)
    {
        _context = context;
        _query = _context.Transactions.AsQueryable();
    }

    public IQueryable<Transaction> Build() => _query;

    // Filters
    public ITransactionQueryBuilder ForUser(int userId, bool isAdmin = false, List<int> sharedAccountIds = null)
    {
        if (!isAdmin)
        {
            if (sharedAccountIds != null && sharedAccountIds.Any())
                _query = _query.Where(t => t.UserId == userId || sharedAccountIds.Contains(t.AccountId));
            else
                _query = _query.Where(t => t.UserId == userId);
        }
        return this;
    }

    public ITransactionQueryBuilder WithDateRange(DateTime? from, DateTime? to)
    {
        if (from.HasValue) _query = _query.Where(t => t.CreatedAt >= from.Value);
        if (to.HasValue) _query = _query.Where(t => t.CreatedAt <= to.Value);
        return this;
    }

    public ITransactionQueryBuilder WithAmountRange(decimal? min, decimal? max, decimal? exact = null)
    {
        if (exact.HasValue)
            _query = _query.Where(t => t.Amount == exact.Value);
        else
        {
            if (min.HasValue) _query = _query.Where(t => t.Amount >= min.Value);
            if (max.HasValue) _query = _query.Where(t => t.Amount <= max.Value);
        }
        return this;
    }

    public ITransactionQueryBuilder WithTypes(List<int> typeIds)
    {
        if (typeIds != null && typeIds.Any())
            _query = _query.Where(t => typeIds.Contains(t.TransactionTypeId));
        return this;
    }

    public ITransactionQueryBuilder WithStatuses(List<int> statusIds)
    {
        if (statusIds != null && statusIds.Any())
            _query = _query.Where(t => statusIds.Contains(t.TransactionStatusId));
        return this;
    }

    public ITransactionQueryBuilder SearchRecipient(string recipientQuery)
    {
        if (!string.IsNullOrWhiteSpace(recipientQuery))
        {
            _query = _query.Where(t =>
                t.RecipientName.ToLower().Contains(recipientQuery.ToLower()) ||
                t.RecipientEmail.ToLower().Contains(recipientQuery.ToLower())
            );
        }
        return this;
    }

    public ITransactionQueryBuilder SearchDescription(string descriptionQuery, DateTime? fromDate = null)
    {
        if (!string.IsNullOrWhiteSpace(descriptionQuery))
        {
            if (fromDate.HasValue && (DateTime.UtcNow - fromDate.Value).TotalDays > 90)
                throw new InvalidOperationException("Full-text search is limited to the last 90 days.");

            _query = _query.Where(t => t.Description.ToLower().Contains(descriptionQuery.ToLower()));
        }
        return this;
    }

    public ITransactionQueryBuilder WithPaymentMethods(List<int> paymentMethodIds)
    {
        if (paymentMethodIds != null && paymentMethodIds.Any())
            _query = _query.Where(t => paymentMethodIds.Contains(t.PaymentMethodId));
        return this;
    }

    public ITransactionQueryBuilder WithTags(List<int> tagIds)
    {
        if (tagIds != null && tagIds.Any() && _context != null)
        {
            _query = _query.Where(t => _context.TransactionTags
                           .Where(tt => tagIds.Contains(tt.TagId))
                           .Select(tt => tt.TransactionId)
                           .Contains(t.Id));
        }
        return this;
    }

    // Sorting 
    public ITransactionQueryBuilder SortBy(string sortBy, string sortDirection)
    {
        bool desc = sortDirection?.ToLower() == "desc";

        _query = sortBy?.ToLower() switch
        {
            "status" => _query.OrderBy(t => StatusOrder.ContainsKey(t.TransactionStatusId)
                ? StatusOrder[t.TransactionStatusId]
                : int.MaxValue),
            "date" => desc ? _query.OrderByDescending(t => t.CreatedAt) : _query.OrderBy(t => t.CreatedAt),
            "amount" => desc ? _query.OrderByDescending(t => t.Amount) : _query.OrderBy(t => t.Amount),
            _ => _query.OrderByDescending(t => t.CreatedAt)
        };

        return this;
    }

    public ITransactionQueryBuilder Paginate(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        _query = _query.Skip((page - 1) * pageSize).Take(pageSize);
        return this;
    }

    public ITransactionQueryBuilder LimitResults(int maxResults = 10000)
    {
        _query = _query.Take(maxResults);
        return this;
    }

    // Status Order
    private static readonly Dictionary<int, int> StatusOrder = new()
    {
        { 1, 0 }, // Pending
        { 2, 1 }, // Completed
        { 3, 2 }, // Failed
        { 4, 3 }  // Cancelled
    };
}