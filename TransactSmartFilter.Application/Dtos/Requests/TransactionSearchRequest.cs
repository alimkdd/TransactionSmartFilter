namespace TransactSmartFilter.Application.Dtos.Requests;

public record TransactionSearchRequest
(
    int UserId,

    // Date Range
    DateTime? FromDate,
    DateTime? ToDate,
    string PredefinedRange,

    // Amount
    decimal? MinAmount,
    decimal? MaxAmount,
    decimal? ExactAmount,

    // Filters
    List<int> TransactionTypeIds,
    List<int> TransactionStatusIds,
    string Recipient,
    string Description,
    List<int> PaymentMethodIds,
    List<int> TagIds,

    // Pagination
    int Page = 1,
    int PageSize = 20,

    // Sorting
    string SortBy = "Date",
    string SortDirection = "desc"
);