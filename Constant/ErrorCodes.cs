namespace DexCexMevBot.Constant;

public static class ErrorCodes
{
    public const int DEPOSIT_IS_DISABLED_ERROR_CODE = 708;
    public const int WITHDRAWAL_IS_DISABLED_ERROR_CODE = 709;
    public const int PROFIT_IS_TOO_SMALL = 748;
    
    public const int THE_PRICE_WAS_HIGHLY_CHANGED_TO_BAD_SIDE = 409;
    public const int NOT_PROFITABLE_TO_OUTBID_BEST_MEMPOOL_COMPETITOR = 10000;
    public const int TRANSACTION_WAS_NOT_MINED_IN_EXPECTED_BLOCKS = 10001;
    public const int TARGET_TRANSACTION_WAS_MINED_WITHOUT_OURS = 10002;
    public const int NOT_PROFITABLE_TO_START_TRADE_DUE_TO_MARKET_GAS_PRICE = 10003;
    public const int NOT_ENOUGH_BALANCE_TO_PROCESS_TASK = 10004;
    public const int SOMEONE_BOUGHT_BEFORE = 10005;
    public const int SOMEONE_TRANSFERRED_BEFORE = 10006;
    public const int BASE_FEE_IS_MORE_THAN_BALANCE_DIFFERENCE = 882993;
    public const int SIMULATION_FAILED = 589;
    
    
    // Processor
    public const int ADDRESS_IS_NOT_WHITELISTED = 10007;
    public const int MANUALLY_INTERRUPTED = 10008;
    public const int NOT_FOUND = 404;
}