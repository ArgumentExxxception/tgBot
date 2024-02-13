using System.Net;
using System.Text.Json;
using Newtonsoft.Json;
using tgBot.Models;
using tgBot.Services.Interfaces;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace tgBot.Services;

public class CoinGeckoApiClient : ICoinGeckoApiClient
{
    private const int CoinsUpdateFrequencyInHours = 2;
    private DateTime _lastCoinIdsUpdated;
    private List<CoinsIds>? _cashedCoinIds = new();
    private readonly HttpClient _httpClient;

    public CoinGeckoApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _lastCoinIdsUpdated = DateTime.MinValue;
    }

    private readonly JsonSerializerOptions? _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<List<CoinInfoById>?> GetCoinInfoById(string id, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&ids={id}&order=market_cap_desc&per_page=100&page=1&sparkline=false&locale=en")
        {
            Headers =
            {
                {
                    "user-agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36"
                }
            }
        };
        var result = await _httpClient.SendAsync(request);

        if (result.StatusCode != HttpStatusCode.OK) return null;
        var test = await result.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<CoinInfoById>>(test, _options);

    }

    public async Task<CoinForDate?> GetCoinInfoForDate(string id, string date, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.coingecko.com/api/v3/coins/{id}/history?date={date}")
        {
            Headers =
            {
                {
                    "user-agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36"
                }
            }
        };
        var result = await _httpClient.SendAsync(request);
        if (result.StatusCode == HttpStatusCode.OK)
        {
            return await JsonSerializer.DeserializeAsync<CoinForDate>
            (await result.Content.ReadAsStreamAsync(cancellationToken), 
                _options, 
                cancellationToken);
        }

        return null;
    }

    public async Task<List<CoinsIds>?> GetCoinsIds(CancellationToken cancellationToken)
    {
        //TODO const, refactor
        if (_lastCoinIdsUpdated + TimeSpan.FromHours(CoinsUpdateFrequencyInHours) > DateTime.UtcNow ||
            _cashedCoinIds != null &&
            _cashedCoinIds.Count != 0)
        {
            return _cashedCoinIds;
        }

        var request = new HttpRequestMessage(HttpMethod.Get,
            "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=100&page=1&sparkline=false&locale=en")
        {
            Headers =
            {
                {
                    "user-agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36"
                }
            }
        };
        var result = await _httpClient.SendAsync(request);
        if (result.StatusCode != HttpStatusCode.OK) return null;
        _cashedCoinIds = await JsonSerializer.DeserializeAsync<List<CoinsIds>>
        (await result.Content.ReadAsStreamAsync(cancellationToken),
            _options,
            cancellationToken);
        _lastCoinIdsUpdated = DateTime.UtcNow;
        return _cashedCoinIds;
    }

    public async Task<List<CoinChart>?> GetCoinChart(string id, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.coingecko.com/api/v3/coins/{id}/market_chart?vs_currency=usd&days=2")
        {
            Headers =
            {
                {
                    "user-agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36"
                }
            }
        };
        var result = await _httpClient.SendAsync(request);
        return result.StatusCode == HttpStatusCode.OK
            ? await JsonSerializer.DeserializeAsync<List<CoinChart>>
            (await result.Content.ReadAsStreamAsync(cancellationToken),
                _options,
                cancellationToken)
            : null;
    }
}