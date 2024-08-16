using DexCexMevBot.Modules.Estimator.Models.Estimator;
using DexCexMevBot.Modules.Estimator.PriceUpdateHandlers.OrderBook;
using DexCexMevBot.Modules.Orberbooks.CexClients.Abstractions;
using DexCexMevBot.Modules.Orberbooks.CexClients.Proxy;
using DexCexMevBot.Modules.Orberbooks.Contracts;
using DexCexMevBot.Utils;
using ILogger = Serilog.ILogger;

namespace DexCexMevBot.Modules.Orberbooks.Pollers;

public class PollerService
{
    private const string SYMBOL = "AAVE/ETH";
    private const string EXCHANGE_ID = "BINANCE";
    private const int ORDERBOOK_POOLING_INTERVAL_MS = 2000;
    // 3 min
    private const int PROXY_POOLING_INTERVAL_MS = 180000;
    
    private readonly ProxyProvider _proxyProvider;
    private readonly AbstractCexClient _abstractCexClient;
    private readonly OrderBookService _orderBookService;
    private readonly ILogger _logger;

    private RotatingProxyList _rotatingProxyList;
    

    public PollerService(ProxyProvider proxyProvider,
        AbstractCexClient abstractCexClient,
        OrderBookService orderBookService)
    {
        _proxyProvider = proxyProvider;
        _abstractCexClient = abstractCexClient;
        _orderBookService = orderBookService;
        _logger = CustomLoggerFactory.CreateLogger("Poller service");
    }

    public Task InitializeOrderBookPollingAsync(CancellationToken cancellationToken)
    {
        // var proxyList = await _proxyProvider.GetActualProxyListAsync(EXCHANGE_ID);
        // _rotatingProxyList = new RotatingProxyList(proxyList);
        //
        // var proxyPoolingTask = Task.Run(async () =>
        // {
        //     try
        //     {
        //         while (!cancellationToken.IsCancellationRequested)
        //         {
        //             await Task.Delay(PROXY_POOLING_INTERVAL_MS, cancellationToken);
        //             proxyList = await _proxyProvider.GetActualProxyListAsync(EXCHANGE_ID);
        //             _rotatingProxyList = new RotatingProxyList(proxyList);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.Warning($"Proxy pooling task error: {ex.Message}");
        //     }
        // }, cancellationToken);
        //
        // var orderBookPollingTask = Task.Run(async () =>
        // {
        //     try
        //     {
        //         while (!cancellationToken.IsCancellationRequested)
        //         {
        //             await PollOrderBookAsync();
        //             await Task.Delay(ORDERBOOK_POOLING_INTERVAL_MS, cancellationToken);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.Warning($"Order book polling task error: {ex.Message}");
        //     }
        // }, cancellationToken);
        //
        // await Task.WhenAll(proxyPoolingTask, orderBookPollingTask);
        
        return Task.Run(async () =>
        {
            var proxyList = await _proxyProvider.GetActualProxyListAsync(EXCHANGE_ID);
            _rotatingProxyList = new RotatingProxyList(proxyList);

            var proxyPoolingTask = Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(PROXY_POOLING_INTERVAL_MS, cancellationToken);
                        proxyList = await _proxyProvider.GetActualProxyListAsync(EXCHANGE_ID);
                        _rotatingProxyList = new RotatingProxyList(proxyList);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Proxy pooling task error: {ex.Message}");
                }
            }, cancellationToken);

            var orderBookPollingTask = Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await PollOrderBookAsync();
                        await Task.Delay(ORDERBOOK_POOLING_INTERVAL_MS, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Order book polling task error: {ex.Message}");
                }
            }, cancellationToken);

            await Task.WhenAll(proxyPoolingTask, orderBookPollingTask);
        }, cancellationToken);
    }
    
    private async Task PollOrderBookAsync(int retries = 0)
    {
        var startProcessingTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var proxy = _rotatingProxyList.GetNextProxy();
        try
        {
            var orderBook = await _abstractCexClient.GetOrderBookAsync(SYMBOL, proxy);

            var orderBookDto = new OrderBookUpdateDto(EXCHANGE_ID,
                SYMBOL,
                orderBook,
                "dex/cex",
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 
                startProcessingTimestamp);

            // TODO: Raise estimation method
            await _orderBookService.OrderBookUpdateAsync(new Estimator.PriceUpdateHandlers.OrderBook.Dto.OrderBookUpdateDto
            {
                ExchangeId = orderBookDto.ExchangeId,
                PublishTimestamp = orderBookDto.PublishTimestamp,
                StartProcessingTimestamp = startProcessingTimestamp,
                Symbol = orderBookDto.Symbol,
                OrderBook = new OrderBookDto
                {
                    Source = "http",
                    Bids = orderBookDto.OrderBook.Bids,
                    Asks = orderBookDto.OrderBook.Asks,
                    Timestamp = orderBookDto.OrderBook.Timestamp
                }
            });
        }
        catch (Exception e)
        {
            // if (retries < 1 && 
            //     (e.Message.Contains("ECONNRESET") || 
            //      e.Message.Contains("ETIMEDOUT") || 
            //      e.Message.Contains("EHOSTUNREACH") || 
            //      e.Message.Contains("ECONNREFUSED")))
            {
                // TODO: handling exception proper
                _rotatingProxyList.DeleteInactiveProxy(proxy);
                await PollOrderBookAsync(++retries);
                _logger.Warning($"Failed polling ${SYMBOL} order book: ${e.Message}, proxy: ${proxy}");
            }
        }
    }
}