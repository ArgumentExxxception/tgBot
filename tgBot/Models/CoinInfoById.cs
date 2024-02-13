using System.Text.Json.Serialization;

namespace tgBot.Models;

public record CoinInfoById(
    [property:JsonPropertyName("id")] string id,
    [property:JsonPropertyName("current_price")] double currentPrice,
    [property:JsonPropertyName("high_24h")] double highPriceForDay,
    [property:JsonPropertyName("low_24h")] double lowPriceForDay,
    [property:JsonPropertyName("price_change_percentage_24h")] double priceChangePercentage);