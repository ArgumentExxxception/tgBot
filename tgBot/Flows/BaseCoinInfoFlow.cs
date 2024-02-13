using System.Net.Mime;
using System.Threading.Tasks.Dataflow;
using Telegram.Bot;
using Telegram.Bot.Types;
using tgBot.Services;
using tgBot.Services.Interfaces;

namespace tgBot;

public enum BaseCoinInfoFlowStates
{
    SetCoinId,
    GetCoinHistoryInfo
}

public class CoinFlowData : IFlowData
{
    public BaseCoinInfoFlowStates State { get; set; }

    public Flows Flow => Flows.Coins;

    public string Id { get; set; } = null!;
}
public class BaseCoinInfoFlow: IFlowUpdateHandler<CoinFlowData>
{
    private readonly ICoinGeckoApiClient _coinGeckoApi;

    public BaseCoinInfoFlow(ICoinGeckoApiClient coinGeckoApi)
    {
        _coinGeckoApi = coinGeckoApi;
    }

    public async Task OnFlowStarted(ITelegramBotClient botClient, UserData userData, Update update, CancellationToken cancellationToken)
    {
        if (userData.FlowData is not CoinFlowData coinFlowData)
        {
            throw new InvalidOperationException();
        }

        coinFlowData.State = BaseCoinInfoFlowStates.SetCoinId;
    }

    public async Task<IFlowData?> HandleUpdate(ITelegramBotClient botClient, UserData userData, Update update, CancellationToken cancellationToken)
    {
        if (userData.FlowData is not CoinFlowData coinFlowData)
        {
            throw new InvalidOperationException();
        }

        switch (coinFlowData.State)
        {
            case BaseCoinInfoFlowStates.SetCoinId:
                await botClient.SendTextMessageAsync(
                    chatId: userData.Id,
                    text: "Укажите Id коина",
                    cancellationToken: cancellationToken);
                coinFlowData.State = BaseCoinInfoFlowStates.SetCoinId;
                coinFlowData.State = BaseCoinInfoFlowStates.GetCoinHistoryInfo;
                break;
            case BaseCoinInfoFlowStates.GetCoinHistoryInfo:
                coinFlowData.Id = update.Message?.Text.ToLower();
                var result = await _coinGeckoApi.GetCoinInfoById(coinFlowData.Id,cancellationToken);
                if (result.Count != 0)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: userData.Id,
                        text: $"""
                              Информация о {result[0].id}
                              Актуальная цена: {result[0].currentPrice}
                              Максимальная цена за 24 часа: {result[0].highPriceForDay}
                              Минимальная цена за 24 часа: {result[0].lowPriceForDay}
                              Изменение цены: {result[0].priceChangePercentage} %
                              """,
                        cancellationToken: cancellationToken);
                    return new HelloFlow();
                }
                await botClient.SendTextMessageAsync(
                    chatId: userData.Id,
                    text: "Что то пошло не так( Попробуйте снова",
                    cancellationToken: cancellationToken);
                coinFlowData.State = BaseCoinInfoFlowStates.SetCoinId;
                break;
        }

        return null;
    }
}
//TODO Валидация текста!