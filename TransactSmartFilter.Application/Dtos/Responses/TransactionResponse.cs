namespace TransactSmartFilter.Application.Dtos.Responses;

public record TransactionResponse
(
    int Id,
    int AccountId,
    decimal Amount,
    int TransactionTypeId,
    int TransactionStatusId,
    int PaymentMethodId,
    string RecipientName,
    string RecipientEmail,
    string Description,
    DateTime CreatedAtUtc,
    List<string> Tags
);