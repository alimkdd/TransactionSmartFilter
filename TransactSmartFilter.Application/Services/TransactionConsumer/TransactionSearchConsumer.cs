using MassTransit;
using System.Text.Json;
using TransactSmartFilter.Application.Dtos.Requests;
using TransactSmartFilter.Application.Interfaces;
using TransactSmartFilter.Domain.Enums;
using TransactSmartFilter.Infrastructure.Context;

namespace TransactSmartFilter.Application.Services.TransactionConsumer;

public class TransactionSearchConsumer : IConsumer<TransactionSearchJobRequest>
{
    private readonly AppDbContext _dbContext;
    private readonly ITransactionService _transactionService;

    public TransactionSearchConsumer(AppDbContext dbContext, ITransactionService transactionService)
    {
        _dbContext = dbContext;
        _transactionService = transactionService;
    }

    public async Task Consume(ConsumeContext<TransactionSearchJobRequest> context)
    {
        var message = context.Message;
        var job = await _dbContext.TransactionSearchJobs.FindAsync(message.JobId);
        if (job == null) return;

        try
        {
            var request = JsonSerializer.Deserialize<TransactionSearchRequest>(message.RequestJson);
            var result = await _transactionService.Search(request);

            job.ResultJson = JsonSerializer.Serialize(result);
            job.Status = Status.Completed.ToString();
            job.CompletedAt = DateTime.UtcNow;
        }
        catch
        {
            job.Status = Status.Failed.ToString();
            job.CompletedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
    }
}