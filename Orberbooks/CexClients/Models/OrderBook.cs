namespace DexCexMevBot.Modules.Orberbooks.CexClients.Models;

public class OrderBook
{
    public decimal[][] Bids { get; }
    public decimal[][] Asks { get; }
    public long Timestamp { get; }

    public OrderBook(decimal[][] bids, decimal[][]asks, long timestamp)
    {
        Bids = bids ?? throw new ArgumentNullException(nameof(bids));
        Asks = asks ?? throw new ArgumentNullException(nameof(asks));
        Timestamp = timestamp;
    }
}