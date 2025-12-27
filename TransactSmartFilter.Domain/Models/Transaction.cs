namespace TransactSmartFilter.Domain.Models;

public class Transaction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int AccountId { get; set; }

    public decimal Amount { get; set; }

    public int TransactionTypeId { get; set; }

    public int TransactionStatusId { get; set; }

    public int PaymentMethodId { get; set; }

    public string RecipientName { get; set; }

    public string RecipientEmail { get; set; }

    public string Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
    public Account Account { get; set; }
    public TransactionType TransactionType { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ICollection<TransactionTag> TransactionTags { get; set; }
}