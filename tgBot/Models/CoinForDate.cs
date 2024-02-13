using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace tgBot.Models;

public record CurrentPrice
{
    public double usd { get; set; }
}

public record MarkerData
{
    [property: JsonPropertyName("current_price")]
    public CurrentPrice CurrentPrice { get; set; }
    
    [property: JsonPropertyName("market_cap")]
    public MarkerCap MarkerCap { get; set; }
}

public record MarkerCap 
{
    public double usd { get; set; }
}

public record CoinForDate
{
    public string Id { get; set; }
    [property: JsonPropertyName("market_data")]public MarkerData MarkerData { get; set; }
};