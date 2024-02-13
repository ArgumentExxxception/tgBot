using System.Diagnostics.CodeAnalysis;

namespace tgBot;

public class User
{
    public long Id { get; set; }
    public List<string>? CoinIdList { get; set; }
    public string Name { get; set; }
}