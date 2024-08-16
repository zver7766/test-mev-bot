namespace DexCexMevBot.Constant;

public static class Exchanges
{
    public const string BINANCE = "binance";

    public const string ETH_EXCHANGE = "eth";
  
    public static readonly Dictionary<string, int> BLOCK_CACHE_SIZE = new()
    {
        { ETH_EXCHANGE, 2 },
    };

    public static readonly Dictionary<string, int> INITIALIZATION_BATCH_SIZE = new()
    {
        { ETH_EXCHANGE, 50 },
    };
    
    public static readonly string[] CEX_EXCHANGES =
    {
        BINANCE
    };
    
    public static readonly string[] DEX_EXCHANGES =
    {
        ETH_EXCHANGE,
    };

    public static readonly Dictionary<string, Dictionary<string, int>> CEX_DEPOSIT_BLOCK_CONFIRMATIONS_COUNT = new()
    {
        [ETH_EXCHANGE] = new()
        {
            { BINANCE, 9 },
            // { BINANCE_INJECTIVE, 9 },
        },
    };
    
    public static readonly string[] LIQUID_CEX_EXCHANGES =
    {
        BINANCE,
    };
    
    public static string[] EVM_DEX_EXCHANGES =
    {
        ETH_EXCHANGE,
    };
}