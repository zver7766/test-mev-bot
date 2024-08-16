namespace DexCexMevBot.Constant;

public class Common
{
    public static string NETWORK_NAME { get; set; }
    public static NetworkName NETWORK_NAME_ENUM { get; set; }
    public static NetworkChainId NETWORK_ID { get; set; }

    public static List<string> WS_LINKS =
    [
        "wss://ws-geth-1.peanut.dysnix.online",
        "wss://ws-geth-2.peanut.dysnix.online",
        "wss://ws-geth-3.peanut.dysnix.online",
        "wss://ws-geth-4.peanut.dysnix.online",
        "wss://ws-geth-5.peanut.dysnix.online",
        "wss://ws-geth-6.peanut.dysnix.online"
    ];
    
    public static long APPROVE_TX_CACHE_TIME {
        get
        {
            switch (NETWORK_NAME_ENUM)
            {
                case NetworkName.bsc:
                    return 12 * 3 * 1000;
                default:
                    return 12 * 12 * 1000;
            }
        }
    }
    
    // BLOXROUTE
    public static readonly string BLOXROUTE_WS_LINK_ETH = "wss://pa-bxgw.peanut.trade/ws";
    // public static readonly string BLOXROUTE_WS_LINK_BSC = "wss://germany.bsc.blxrbdn.com/ws";
    public static readonly string BLOXROUTE_ACCOUNT_ID = "3b0a49dd-8807-4f55-a5e7-3aa118f17992:7d2e63a408acc4d9d9e4cff9228d1388";
    
    public const string SymbolSeparator = "/";
    
    // estimator
    public static string BuyExchangeId;

    public static readonly string[] SupportedCexList =
    {
        "binance"
    };
    
    public static readonly Dictionary<string, decimal> MAX_DEX_MARKET_LATENCY_MS = new()
    {
        { Exchanges.ETH_EXCHANGE, 15000 },
        // { BSC_EXCHANGE, 5000 },
    };

    public static readonly int MAX_CEX_LATENCY_MS = 25000;
    public const int WEI_IN_GWEI = 1_000_000_000;
    public const decimal HUGE_PROFIT_PERCENT = 0.2M;

}