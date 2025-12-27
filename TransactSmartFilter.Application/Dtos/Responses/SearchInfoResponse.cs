namespace TransactSmartFilter.Application.Dtos.Responses;

public record SearchInfoResponse
(
    Guid JobId,
    TimeSpan QueryTime,
    string AppliedFilters
);