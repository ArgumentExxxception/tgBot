namespace tgBot;

public enum Flows
{
    Register,
    PriceChange,
    CoinDate,
    Coins,
    New
}

public interface IFlowData
{
    public Flows Flow { get; }
}
public class UserData
{
    public long Id { get; set; }
    public IFlowData? FlowData { get; set; }
    
}