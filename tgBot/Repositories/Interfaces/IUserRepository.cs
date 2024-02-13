namespace tgBot.Repositories.Interfaces;

public interface IUserRepository
{
    public Task<User?> GetUserById(long id);
    public Task<User> AddUserAsync(User user);

    public Task<bool> AddCoinsIds(List<string>? ids, long id);
    Task<List<string>> GetAllTrackingCoins();
    Task<IEnumerable<User>> GetUsersWithTrackedCoins(Dictionary<string, decimal> coinPriceChanges);
    public Task UpdateUserAsync(User user);
    public Task<IReadOnlyList<User>> GetAllUsers();
}