using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace tgBot;

public class FlowManager
{
    private readonly IServiceProvider _serviceProvider;

    public FlowManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task HandleUpdate(ITelegramBotClient botClient,UserData userData,Update update ,CancellationToken cancellationToken)
    {
        if (userData.FlowData == null)
        {
            return;
        }
        using var scope = _serviceProvider.CreateScope();
        var handler = GetHandler(scope.ServiceProvider, userData);
        
        var newHandler = await handler
            .HandleUpdate(botClient, userData, update, cancellationToken);
        
        if (newHandler != null)
        {
            userData.FlowData = newHandler;
            handler = GetHandler(scope.ServiceProvider, userData);
            await handler.OnFlowStarted(
                botClient,
                userData,
                update,
                cancellationToken);
        }
    }

    public static IFlowUpdateHandler? GetHandler(IServiceProvider serviceProvider, UserData userData)
    {
        if (userData.FlowData == null)
        {
            return null;
        }
        return serviceProvider.GetRequiredService(
                typeof(IFlowUpdateHandler<>).MakeGenericType(userData.FlowData.GetType())) 
            as IFlowUpdateHandler;
    }
}