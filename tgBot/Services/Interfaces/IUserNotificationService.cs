namespace tgBot.Services.Interfaces;

public interface IUserNotificationService
{
    Task NotificateUser(CancellationToken cancellationToken);
    Task<Dictionary<string,decimal>?> CalculatePriceDifference(CancellationToken cancellationToken);
}