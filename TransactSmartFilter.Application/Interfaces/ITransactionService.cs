using TransactSmartFilter.Application.Dtos.Requests;
using TransactSmartFilter.Application.Dtos.Responses;

namespace TransactSmartFilter.Application.Interfaces;

public interface ITransactionService
{
    Task<TransactionSearchResponse> Search(TransactionSearchRequest request);

    Task<TransactionSearchResponse> Job(Guid jobId);
}