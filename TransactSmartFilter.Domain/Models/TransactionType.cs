namespace TransactSmartFilter.Domain.Models;

public class TransactionType
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Transaction> Transactions { get; set; }
}