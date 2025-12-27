namespace TransactSmartFilter.Application.Dtos.Requests;

public record TransactionSearchJobRequest
(
    Guid JobId,
    int AccountId,
    string RequestJson
);