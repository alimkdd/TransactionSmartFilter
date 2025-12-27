namespace TransactSmartFilter.Application.Interfaces;

public interface IUserTierService
{
    Task<string> GetUserTier(int userId);

    Task<bool> IsAdmin(int userId);

    Task<List<int>> GetSharedAccountIds(int userId);
}