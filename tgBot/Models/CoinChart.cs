using System.Text.Json.Serialization;

namespace tgBot.Models;

public record CoinChart(
    long TimeStamp,
    decimal Price
    );