namespace TransactSmartFilter.Domain.Models;

public class TransactionSearchJob
{
    public Guid Id { get; set; }
    public int AccountId { get; set; }
    public string RequestJson { get; set; }
    public string ResultJson { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}