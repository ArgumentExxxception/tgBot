using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using tgBot.Models;
using tgBot.Repositories;
using tgBot.Repositories.Interfaces;
using tgBot.Services;
using tgBot.Services.Interfaces;

namespace tgBot;

public class PriceChangeFlowData : IFlowData
{
    public List<CoinsIds>? Ids { get; set; }
    public Flows Flow => Flows.PriceChange;
    public int Page { get; set; } = 0;
    public List<string> UserIds = new();
}

public class PriceChangeSubscriptionFlow : IFlowUpdateHandler<PriceChangeFlowData>
{
    private readonly IUserRepository _userRepository;
    private readonly ICoinGeckoApiClient _coinGeckoApi;
    private const string Left = "⬅️";
    private const string Right = "➡️";

    private static KeyboardButton[] Navigate = new[]
    {
        new KeyboardButton(Left),
        new KeyboardButton(Right)
    };

    public PriceChangeSubscriptionFlow(ICoinGeckoApiClient coinGeckoApi, IUserRepository userRepository)
    {
        _coinGeckoApi = coinGeckoApi;
        _userRepository = userRepository;
    }

    public async Task OnFlowStarted(ITelegramBotClient botClient, UserData userData, Update update,
        CancellationToken cancellationToken)
    {
        if (userData.FlowData is not PriceChangeFlowData priceChangeFlowData)
        {
            throw new InvalidOperationException();
        }
    }

    public async Task<IFlowData?> HandleUpdate(ITelegramBotClient botClient, UserData userData, Update update,
        CancellationToken cancellationToken)
    {
        if (userData.FlowData is not PriceChangeFlowData priceChangeFlowData)
        {
            throw new InvalidOperationException();
        }

        switch (update.Message.Text)
        {
            case Left:
                priceChangeFlowData.Page--;
                break;
            case Right:
                priceChangeFlowData.Page++;
                break;
            case "stop":
                if (await _userRepository.AddCoinsIds(priceChangeFlowData.UserIds, userData.Id))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: userData.Id,
                        text: "Успешно! Начали отслеживание😎",
                        replyMarkup: GetPage(priceChangeFlowData),
                        cancellationToken: cancellationToken);
                    return new HelloFlow();
                }
                await botClient.SendTextMessageAsync(
                    chatId: userData.Id,
                    text: "Что то пошло не так, попробуйте снова",
                    replyMarkup: GetPage(priceChangeFlowData),
                    cancellationToken: cancellationToken);
                return new HelloFlow();
            default:
                await botClient.SendTextMessageAsync(
                    chatId: userData.Id,
                    text: $"Вы выбрали {update.Message.Text}" +
                          $"Чтобы выйти напишите 'stop'",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
                break;
        }
        if (update.Message.Text.ToLower() != "⬅️" 
            && update.Message.Text.ToLower() != "➡️"  
            && update.Message.Text.ToLower() != "/tracking" 
            && update.Message.Text.ToLower() != "stop")
        {
            priceChangeFlowData.UserIds.Add(update.Message.Text);
        }
        priceChangeFlowData.Ids = await _coinGeckoApi.GetCoinsIds(cancellationToken);

        await SendPage(botClient, userData, cancellationToken, priceChangeFlowData, update);
        return null;
    }

    private async Task SendPage(ITelegramBotClient botClient, UserData userData, CancellationToken cancellationToken,
        PriceChangeFlowData priceChangeFlowData, Update update)
    {
        await botClient.SendTextMessageAsync(
            chatId: userData.Id,
            text: $"Выберите id коина который хотите отслеживать (страница {priceChangeFlowData.Page + 1})",
            replyMarkup: GetPage(priceChangeFlowData),
            cancellationToken: cancellationToken);
    }

    private IReplyMarkup GetPage(PriceChangeFlowData priceChangeFlowData)
    {
        var ls = priceChangeFlowData.Ids
            .Skip(priceChangeFlowData.Page * 9)
            .Take(9)
            .Select(x => new KeyboardButton(x.Id.ToString()))
            .Chunk(3);

        return new ReplyKeyboardMarkup(ls.Concat(new[] { GetNavigation(priceChangeFlowData).ToArray() }));
    }

    private IEnumerable<KeyboardButton> GetNavigation(PriceChangeFlowData priceChangeFlowData)
    {
        if (priceChangeFlowData.Page != 0)
        {
            yield return Navigate[0];
        }

        if (priceChangeFlowData.Ids.Count / 9 > priceChangeFlowData.Page)
        {
            yield return Navigate[1];
        }
    }
}