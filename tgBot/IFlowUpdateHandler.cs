using Telegram.Bot;
using Telegram.Bot.Types;

namespace tgBot;

public interface IFlowUpdateHandler
{
    public Task OnFlowStarted(
        ITelegramBotClient botClient,
        UserData userData,
        Update update,
        CancellationToken cancellationToken);
    
    public Task<IFlowData?> HandleUpdate(
        ITelegramBotClient botClient,
        UserData userData,
        Update update,
        CancellationToken cancellationToken);
}
public interface IFlowUpdateHandler<T>: IFlowUpdateHandler where T: IFlowData
{
}