using DexCexMevBot.Modules.Estimator.Routes;
using DexCexMevBot.Modules.Estimator.Services;
using DexCexMevBot.Modules.Mempool.Abstraction;
using DexCexMevBot.Modules.Mempool.Abstraction.TxSubscribers;
using DexCexMevBot.Modules.Mempool.BlockchainProvider;
using DexCexMevBot.Modules.Mempool.BlockInfoProvider;
using DexCexMevBot.Modules.Mempool.FeeKeeper;
using DexCexMevBot.Modules.Mempool.Tx;
using DexCexMevBot.Modules.Orberbooks.Pollers;
using DexCexMevBot.Modules.Reserves.BlockchainProvider;
using DexCexMevBot.Modules.Reserves.Blocks.Abstractions;
using DexCexMevBot.Modules.Reserves.Pools;
using DexCexMevBot.Modules.Reserves.Pools.Abstractions;
using DexCexMevBot.Modules.Reserves.Tokens;
using DexCexMevBot.Utils;
using ILogger = Serilog.ILogger;

namespace DexCexMevBot.AppInitializer;

public class AppInitializer : IHostedService, IDisposable
{
    private readonly ILogger _logger;
    // Reserves
    private readonly IProvidersPool _providersPool;
    private readonly IBlockHandlerService _blockHandlerService;
    private readonly TokenDataProvider _tokenDataProvider;
    private readonly IPoolsInfoProvider _poolsInfoProvider;
    private readonly PoolsInitializerService _poolsInitializerService;
    private readonly PollerService _pollerService;
    private readonly StreamingBlockchainProvider _streamingBlockchainProvider;
    private readonly FeeKeeperService _feeKeeperService;
    private readonly AbstractBlockInfoProvider _blockInfoProvider;
    private readonly AbstractSubscriber _abstractSubscriber;
    private readonly AbstractBlockchainProvider _blockchainProvider;
    private readonly TxHandler _txHandler;
    private readonly RoutesService _routesService;
    private readonly BaseFeePredictionService _baseFeePredictionService;

    //

    public AppInitializer(
        IProvidersPool providersPool,
        IBlockHandlerService blockHandlerService,
        TokenDataProvider tokenDataProvider,
        IPoolsInfoProvider poolsInfoProvider,
        PoolsInitializerService poolsInitializerService,
        PollerService pollerService,
        StreamingBlockchainProvider streamingBlockchainProvider,
        
        FeeKeeperService feeKeeperService,
        AbstractBlockInfoProvider blockInfoProvider,
        AbstractSubscriber abstractSubscriber,
        AbstractBlockchainProvider blockchainProvider,
        TxHandler txHandler,
        
        RoutesService routesService,
        BaseFeePredictionService baseFeePredictionService)
    {
        _logger = CustomLoggerFactory.CreateLogger("App Initializer");

        _providersPool = providersPool;
        _blockHandlerService = blockHandlerService;
        _tokenDataProvider = tokenDataProvider;
        _poolsInfoProvider = poolsInfoProvider;
        _poolsInitializerService = poolsInitializerService;
        _pollerService = pollerService;
        _streamingBlockchainProvider = streamingBlockchainProvider;
        _feeKeeperService = feeKeeperService;
        _blockInfoProvider = blockInfoProvider;
        _abstractSubscriber = abstractSubscriber;
        _blockchainProvider = blockchainProvider;
        _txHandler = txHandler;
        _routesService = routesService;
        _baseFeePredictionService = baseFeePredictionService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("OrderBook start initializing");

        _pollerService.InitializeOrderBookPollingAsync(cancellationToken);

        _logger.Information("OrderBook initialized");

        _logger.Information("Mempool start initializing");
        await _abstractSubscriber.InitializeAsync();

        if (_blockInfoProvider is BlockInfoBlockchainProvider)
            await _streamingBlockchainProvider.InitializeAsync();

        await _blockchainProvider.InitializeAsync();

        await _feeKeeperService.InitializeAsync(cancellationToken);
        await _blockInfoProvider.InitializeAsync();

        _txHandler.SubscribeToUpdate();

        _logger.Information("Mempool initialized");

        _logger.Information("Reserves start initializing");

        await _poolsInfoProvider.InitializeAsync();
        await _poolsInitializerService.InitializeAsync();
        await _tokenDataProvider.LoadTokensDataAsync();
        await _providersPool.InitializeProvidersAsync();
        await _blockHandlerService.SubscribeToUpdate();

        _logger.Information("Reserves initialized");

        _logger.Information("Estimator start initializing");

        await _routesService.LoadAsync();
        await _baseFeePredictionService.InitializeAsync();

        _logger.Information("Estimator initialized");


    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Dispose()
    {
    }
}