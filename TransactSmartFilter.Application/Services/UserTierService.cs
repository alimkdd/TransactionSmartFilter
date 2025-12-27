using Microsoft.EntityFrameworkCore;
using TransactSmartFilter.Application.Interfaces;
using TransactSmartFilter.Infrastructure.Context;
using UserTier = TransactSmartFilter.Domain.Enums.UserTier;

namespace TransactSmartFilter.Application.Services;

public class UserTierService : IUserTierService
{
    private readonly AppDbContext _dbContext;

    public UserTierService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsAdmin(int userId)
    {
        try
        {
            var tier = await GetUserTier(userId);
            return tier.Equals(UserTier.Admin.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<string> GetUserTier(int userId)
    {
        try
        {
            var user = await _dbContext.Users
                            .Include(u => u.UserTier)
                            .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            if (user.UserTier == null)
                throw new KeyNotFoundException("User tier not found.");

            return user.UserTier.Name;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<List<int>> GetSharedAccountIds(int userId)
    {
        try
        {
            var sharedAccounts = await _dbContext.UserAccounts
                                       .AsNoTracking()
                                       .Where(ua => ua.UserId == userId)
                                       .Select(ua => ua.AccountId)
                                       .ToListAsync();

            return sharedAccounts;
        }
        catch (Exception)
        {
            throw;
        }
    }
}