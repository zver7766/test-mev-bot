using DexCexMevBot.Modules.Orberbooks.CexClients.Abstractions;
using DexCexMevBot.Modules.Orberbooks.CexClients.Models;
using Newtonsoft.Json.Linq;

namespace DexCexMevBot.Modules.Orberbooks.CexClients;

public class BinanceClient : AbstractCexClient
{
    protected override string BuildRequestLink(string symbol)
    {
        symbol = symbol.Replace("/", "");
        return $"https://api.binance.com/api/v3/depth?limit=500&symbol={symbol}";
    }

    protected override OrderBook ParseResponse(string data)
    {
        var jsonData = JObject.Parse(data);

        var bidsArray = jsonData["bids"].ToObject<JArray>();
        var bids = bidsArray
            .Select(bid => new decimal[] { (decimal)bid[0], (decimal)bid[1] })
            .ToArray();

        var asksArray = jsonData["asks"].ToObject<JArray>();
        var asks = asksArray
            .Select(ask => new decimal[] { (decimal)ask[0], (decimal)ask[1] })
            .ToArray();
        
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return new OrderBook(bids, asks, timestamp);
    }
}