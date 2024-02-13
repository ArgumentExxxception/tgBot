using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using tgBot.Services;
using tgBot.Services.Interfaces;

namespace tgBot;

public enum DateCoinInfoFlowState
{
    SetId,
    SetDate,
    GetCoinHistoryInfo
}

public class DateCoinInfoFlowData : IFlowData
{
    public DateCoinInfoFlowState State { get; set; }
    public Flows Flow => Flows.CoinDate;
    
    public string Id { get; set; } = null!;
    
    public string Date { get; set; } = null!;
}

public class DateCoinInfoFlow: IFlowUpdateHandler<DateCoinInfoFlowData>
{
    private readonly ICoinGeckoApiClient _coinGeckoApi;

    public DateCoinInfoFlow(ICoinGeckoApiClient coinGeckoApi)
    {
        _coinGeckoApi = coinGeckoApi;
    }

    public async Task OnFlowStarted(ITelegramBotClient botClient, UserData userData, Update update, CancellationToken cancellationToken)
    {
        if (userData.FlowData is not DateCoinInfoFlowData dateCoinInfoFlowData)
        {
            throw new InvalidOperationException();
        }

        dateCoinInfoFlowData.State = DateCoinInfoFlowState.SetId;
    }

    public async Task<IFlowData?> HandleUpdate(ITelegramBotClient botClient, UserData userData, Update update, CancellationToken cancellationToken)
    {
        if (userData.FlowData is not DateCoinInfoFlowData dateCoinInfoFlowData)
        {
            throw new InvalidOperationException();
        }

        switch (dateCoinInfoFlowData.State)
        {
            case DateCoinInfoFlowState.SetId:
                await botClient.SendTextMessageAsync(
                    chatId: userData.Id,
                    text: "Укажите Id коина",
                    cancellationToken: cancellationToken);
                dateCoinInfoFlowData.State = DateCoinInfoFlowState.SetId;
                //TODO проверить работу этого flow
                dateCoinInfoFlowData.Id = update.Message?.Text.ToLower();
                dateCoinInfoFlowData.State = DateCoinInfoFlowState.SetDate;
                break;
            case DateCoinInfoFlowState.SetDate:
                await botClient.SendTextMessageAsync(
                    chatId: userData.Id,
                    text: "Укажите дату в формате дд-мм-гггг",
                    cancellationToken: cancellationToken);
                dateCoinInfoFlowData.Id = update.Message?.Text.ToLower();
                dateCoinInfoFlowData.State = DateCoinInfoFlowState.GetCoinHistoryInfo;
                return null;
            case DateCoinInfoFlowState.GetCoinHistoryInfo:
                dateCoinInfoFlowData.Date = update.Message.Text;
                string pattern = @"^(0[1-9]|[12][0-9]|3[01])-(0[1-9]|1[0-2])-\d{4}$";
                if (!await TextValidation.ValidateText(update.Message))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: userData.Id,
                        text: "Я понимаю только текст!",
                        cancellationToken: cancellationToken);
                    dateCoinInfoFlowData.State = DateCoinInfoFlowState.SetDate;
                    break;
                }
                if (!Regex.IsMatch(dateCoinInfoFlowData.Date,pattern))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: userData.Id,
                        text: "Дата в неправильном формате!",
                        cancellationToken: cancellationToken);
                    dateCoinInfoFlowData.State = DateCoinInfoFlowState.SetDate;
                    break;
                }
                var result = await _coinGeckoApi.GetCoinInfoForDate(dateCoinInfoFlowData.Id,dateCoinInfoFlowData.Date,cancellationToken);
                if (result != null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: userData.Id,
                        text: $"Информация о: {result.Id} на {dateCoinInfoFlowData.Date}" +
                              $"Цена: {result.MarkerData.CurrentPrice}" +
                              $"Рыночная капитализация {result.MarkerData.MarkerCap}",
                        cancellationToken: cancellationToken);
                    return new HelloFlow();
                }
                await botClient.SendTextMessageAsync(
                    chatId: userData.Id,
                    text: "Что то пошло не так( Попробуйте снова",
                    cancellationToken: cancellationToken);
                dateCoinInfoFlowData.State = DateCoinInfoFlowState.SetId;
                break;
        }

        return null;
    }
}