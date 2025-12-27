namespace TransactSmartFilter.Domain.Models;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; }

    public string Email { get; set; }

    public int UserTierId { get; set; }

    public DateTime CreatedAt { get; set; }

    public UserTier UserTier { get; set; }

    public ICollection<UserAccount> UserAccounts { get; set; }
}