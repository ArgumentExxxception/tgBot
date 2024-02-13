using tgBot.Models;

namespace tgBot.Services.Interfaces;

public interface ICoinGeckoApiClient
{ 
    Task<List<CoinInfoById>?> GetCoinInfoById(string id, CancellationToken cancellationToken);
    Task<CoinForDate?> GetCoinInfoForDate(string id, string date, CancellationToken cancellationToken);
    Task<List<CoinsIds>?> GetCoinsIds(CancellationToken cancellationToken);
    Task<List<CoinChart>?> GetCoinChart(string id, CancellationToken cancellationToken);
}