using System.Text.Json.Serialization;

namespace tgBot.Models;

public record CoinsIds(
    [property: JsonPropertyName("id")] 
    string Id
    );