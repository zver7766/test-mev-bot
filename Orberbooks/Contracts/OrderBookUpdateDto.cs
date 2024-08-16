using DexCexMevBot.Modules.Orberbooks.CexClients.Models;

namespace DexCexMevBot.Modules.Orberbooks.Contracts;

public class OrderBookUpdateDto
{
    public string ExchangeId { get; }
    public string Symbol { get; }
    public OrderBook OrderBook { get; }
    public string ArbitrageType { get; }
    public long PublishTimestamp { get; }
    public long StartProcessingTimestamp { get; }

    public OrderBookUpdateDto(string exchangeId,
        string symbol,
        OrderBook orderBook,
        string arbitrageType,
        long publishTimestamp,
        long startProcessingTimestamp)

    {
        ExchangeId = exchangeId;
        Symbol = symbol;
        OrderBook = orderBook;
        ArbitrageType = arbitrageType;
        PublishTimestamp = publishTimestamp;
        StartProcessingTimestamp = startProcessingTimestamp;
    }
}