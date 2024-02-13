using Hangfire;
using Telegram.Bot;
using tgBot.Repositories.Interfaces;
using tgBot.Services.Interfaces;

namespace tgBot.Services;

public class NotificationService: IUserNotificationService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserRepository _userRepository;
    private readonly ICoinGeckoApiClient _coinGeckoApi;

    public NotificationService(IUserRepository userRepository, ICoinGeckoApiClient coinGeckoApi, ITelegramBotClient botClient)
    {
        _userRepository = userRepository;
        _coinGeckoApi = coinGeckoApi;
        _botClient = botClient;
    }
    
    public async Task NotificateUser(CancellationToken cancellationToken)
    {
        var coinsPriceChanges = await CalculatePriceDifference(cancellationToken);
        if (coinsPriceChanges == null)
        {
            return;
        }

        foreach (var coinPriceChange in coinsPriceChanges)
        {
            var users = await _userRepository.GetUsersWithTrackedCoins(coinsPriceChanges);
            foreach (var user in users)
            {
                await _botClient.SendTextMessageAsync(
                    chatId: user.Id,
                    text: $""""
                          Внимание!
                          Коин {coinPriceChange.Key} 
                          Изменился в цене на {coinPriceChange.Value}
                          """",
                    cancellationToken: cancellationToken);
            }
        }
    }

    public async Task<Dictionary<string,decimal>?> CalculatePriceDifference(CancellationToken cancellationToken)
    {
        var coinPriceChanges = new Dictionary<string, decimal>();
        var coins = await _userRepository.GetAllTrackingCoins();
        foreach (var coin in coins)
        {
            var coinInfo = await _coinGeckoApi.GetCoinChart(coin,cancellationToken);
            if (coinInfo != null)
            {
                var differencePercentage = (coinInfo[coinInfo.Count].Price - coinInfo[^1].Price)*100/
                                           coinInfo[coinInfo.Count].Price;
                if (Math.Abs(differencePercentage) > 1)
                {
                    coinPriceChanges.Add(coin,differencePercentage);
                }
            }
        }
        return coinPriceChanges;
    }
}