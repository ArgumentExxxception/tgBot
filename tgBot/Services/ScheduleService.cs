using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tgBot.Services.Interfaces;

namespace tgBot.Services;

public class ScheduleService: IHostedService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ScheduleService(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _backgroundJobClient.Schedule<IServiceProvider>(x => SendNotif(x),TimeSpan.FromMinutes(30));
        return Task.CompletedTask;
    }

    private static async Task SendNotif(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<IUserNotificationService>();
        await notificationService.NotificateUser(default);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}