using Microsoft.EntityFrameworkCore;
using TransactSmartFilter.Domain.Models;

namespace TransactSmartFilter.Infrastructure.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionStatus> TransactionStatuses { get; set; }
    public DbSet<TransactionTag> TransactionTags { get; set; }
    public DbSet<TransactionType> TransactionTypes { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<UserTier> UserTiers { get; set; }
    public DbSet<TransactionSearchJob> TransactionSearchJobs { get; set; }
}