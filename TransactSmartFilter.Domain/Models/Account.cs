namespace TransactSmartFilter.Domain.Models;

public class Account
{
    public int Id { get; set; }

    public string AccountNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<UserAccount> UserAccounts { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
}