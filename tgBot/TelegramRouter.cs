using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace tgBot;

public class TelegramRouter: IUpdateHandler
{
    private readonly UserStore _userStore;
    private readonly FlowManager _flowManager;

    public TelegramRouter(UserStore userStore, FlowManager flowManager)
    {
        _userStore = userStore;
        _flowManager = flowManager;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;
        
        var user = _userStore.GetOrCreateUserStatus(message.Chat.Id);

        switch (message.Text.ToLower())
        {
            case "/help":
                await botClient.SendTextMessageAsync(
                    chatId: user.Id,
                    text: """"
                          /help - список доступных комманд
                          /coin - получить актуальную информацию об определенном коине
                          /coindata - получить информацию о коине за дд-мм-гггг
                          /tracking - начать отслеживать цену определенного коина. При значительном изменении
                          цены коина вы будете получать уведомление от бота. Необходима регистрация
                          /reg - зарегистрироваться
                          """",
                    cancellationToken: cancellationToken);
                break;
            case "/coin":
                user.FlowData = new CoinFlowData();
                break;
            case "/coindata":
                user.FlowData = new PriceChangeFlowData();
                break;
            case "/tracking":
                user.FlowData = new PriceChangeFlowData();
                break;
            case "/reg":
                user.FlowData = new RegisterFlowData();
                break;
        }
        

        await _flowManager.HandleUpdate(botClient, user, update, cancellationToken);
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
    }
}