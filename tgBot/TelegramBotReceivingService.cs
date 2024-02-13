using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace tgBot;

public class TelegramBotReceivingService : IHostedService
{
    private CancellationTokenSource _cts = new ();
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly TelegramRouter _telegramRouter;


    public TelegramBotReceivingService(ITelegramBotClient telegramBotClient, TelegramRouter telegramRouter)
    {
        _telegramBotClient = telegramBotClient;
        _telegramRouter = telegramRouter;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _telegramBotClient.StartReceiving(_telegramRouter, cancellationToken: _cts.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
    }
}