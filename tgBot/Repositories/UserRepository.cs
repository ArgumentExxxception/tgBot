using tgBot.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace tgBot.Repositories;

public class UserRepository: IUserRepository
{
    private readonly Context _dbContext;

    public UserRepository(Context dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetUserById(long id)
    {
        return await _dbContext.FindAsync<User>(id);
    }
    
    public async Task<User> AddUserAsync(User user)
    {
        var newUser = await _dbContext.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return newUser.Entity;
    }


    public async Task<bool> AddCoinsIds(List<string>? ids, long id)
    {
        var user = await _dbContext.FindAsync<User>(id);
        if (user != null)
        {
            user.CoinIdList = ids;
            await UpdateUserAsync(user);
            return true;
        }

        return false;
    }
    
    public async Task<List<string>> GetAllTrackingCoins()
    {
        var result = _dbContext.Users.SelectMany(u => u.CoinIdList).Distinct().ToList();
        return result;
    }

    public async Task<IEnumerable<User>> GetUsersWithTrackedCoins(Dictionary<string,decimal> coinPriceChanges)
    {
        var result = _dbContext.Users.Where(u => u.CoinIdList!.Any(coinPriceChanges.ContainsKey));
        return result;
    }

    public async Task<IReadOnlyList<User>> GetAllUsers()
    {
        var allUsers =  _dbContext.Users.Take(_dbContext.Users.Count()).ToList();
        return allUsers;
    }

    public async Task UpdateUserAsync(User user)
    {
        _dbContext.Entry(user).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }
}