namespace TransactSmartFilter.Application.Dtos.Responses;

public record TransactionSearchResponse
(
    List<TransactionResponse> Results,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasMore,
    SearchInfoResponse Metadata
);