namespace TransactSmartFilter.Domain.Models;

public class TransactionTag
{
    public int Id { get; set; }

    public int TransactionId { get; set; }

    public Transaction Transaction { get; set; }

    public int TagId { get; set; }

    public Tag Tag { get; set; }

    public DateTime CreatedAt { get; set; }
}