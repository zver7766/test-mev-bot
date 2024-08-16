namespace DexCexMevBot.Constant;

public static class Currencies
{
    public const string ETH = "ETH";
    public const string BNB = "BNB";
    public const string SOL = "SOL";
    public const string WBTC = "WBTC";
    public const string BTC = "BTC";
    public const string MATIC = "MATIC";
    
    public const string USDT = "USDT";
    public const string USDC = "USDC";
    public const string DAI = "DAI";
    
    public const string VEXT = "VEXT";
    public const string AURORA = "AURORA";
    public const string SQR = "SQR";
    public const string AMB = "AMB";
    public const string HAI = "HAI";
    public const string DEAI = "DEAI";
    public const string SSNC = "SSNC";

    // public static string[] MM_TOKENS = { AURORA, SQR, AMB, HAI, DEAI, SSNC };
    public static string[] STABLECOINS = { USDT, USDC, DAI };
    //
    // public static Dictionary<string, List<string>> ALTCOINS_WITH_BALANCES = new()
    // {
    //     [GATE] = new () { "SLERF", "MEW", "SAFE" }
    // };
    //
    // public static bool IsAltcoinWithBalance(string currencyId, string exchangeId) =>
    //     ALTCOINS_WITH_BALANCES.TryGetValue(exchangeId, out var exchangeAltcoinsWithBalances) &&
    //     exchangeAltcoinsWithBalances.Contains(currencyId);
    //
    // public const string USDT_ETH = "0xdac17f958d2ee523a2206206994597c13d831ec7";
    // public const string USDT_BNB = "0x55d398326f99059ff775485246999027b3197955";
    // public const string USDT_ARB = "0xfd086bc7cd5c481dcc9c85ebe478a1c0b69fcbb9";
    // public const string USDT_POL = "0xc2132d05d31c914a87c6611c10748aeb04b58e8f";
}