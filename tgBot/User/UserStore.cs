using Telegram.Bot.Types;

namespace tgBot;

public class UserStore
{
    private Dictionary<long, UserData> _userStatusMap = new();

    public UserData GetOrCreateUserStatus(long id)
    {
        if (_userStatusMap.TryGetValue(id,out var user ))
        {
            return user;
        }

        var newUser = new UserData()
        {
            Id = id,
            FlowData = new HelloFlow()
        };
         
        _userStatusMap.Add(newUser.Id,newUser);
        return newUser;
    }
}