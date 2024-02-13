using Telegram.Bot;
using Telegram.Bot.Types;

namespace tgBot;

public class HelloFlow: IFlowData
{
    public Flows Flow => Flows.New;
}

public class HelloHandler : IFlowUpdateHandler<HelloFlow>
{
    public Task OnFlowStarted(ITelegramBotClient botClient, UserData userData, Update update,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task<IFlowData?> HandleUpdate(
        ITelegramBotClient botClient,
        UserData userData, 
        Update update,
        CancellationToken cancellationToken)
    {
        if (userData.FlowData is not HelloFlow helloFlow)
            throw new InvalidOperationException();
        
        await botClient.SendTextMessageAsync(
            chatId: userData.Id,
            text: "Привет! Чем могу помочь?",
            cancellationToken: cancellationToken);
        return null;
    }
}