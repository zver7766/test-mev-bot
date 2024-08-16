using DexCexMevBot.Constant;
using static DexCexMevBot.Constant.Common;

namespace DexCexMevBot.Utils;

internal static class CurrencyDirectionResolver
{
    public static (string CurrencyFrom, string CurrencyTo) ResolveCurrenciesFromAndTo(string symbol, OperationSide side)
    {
        var currencies = symbol.Split(SymbolSeparator);

        if (side == OperationSide.Sell)
            return (currencies[0], currencies[1]);
        else
            return (currencies[1], currencies[0]);
    }
}