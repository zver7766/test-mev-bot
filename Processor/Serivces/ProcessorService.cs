using DexCexMevBot.Constant;
using DexCexMevBot.Modules.Estimator.Exceptions;
using DexCexMevBot.Modules.Estimator.Models.Enums;
using DexCexMevBot.Modules.Estimator.Models.Estimator;
using DexCexMevBot.Modules.Estimator.Services;
using Serilog;
using static DexCexMevBot.Utils.CustomLoggerFactory;

namespace DexCexMevBot.Modules.Processor.Serivces;

public class ProcessorService : BackgroundService
{
#if DEBUG
    private const bool IS_DEBUG_ENVIRONMENT = true;
#else
        private const bool IS_DEBUG_ENVIRONMENT = false;
#endif

    private const string ESTIMATES_CHANNEL = "ESTIMATOR_RESULT";
    private const long MAX_ACCEPTABLE_LATENCY_MS = 5000;


    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DexTraderInitializer _dexTraderInitializer;
    
    private readonly SmartConfigurationModule _smartConfigurationModule;


    private readonly LockTaskKeeper _lockTaskKeeper;
    private readonly BlockNumberKeeper _blockNumberKeeper;
    private readonly DexSwapsKeeper _dexSwapsKeeper;
    private readonly TokenTransferKeeper _tokenTransferKeeper;
    private readonly CexDepositAddressKeeper _cexDepositAddressKeeper;
    private readonly TokenTransferChecker _tokenTransferChecker;

    private readonly CexTraderInitializer _cexTraderInitializer;

    public ProcessorService(
        IServiceScopeFactory scopeFactory,
        DexTraderInitializer dexTraderInitializer,
        LockTaskKeeper lockTaskKeeper,
        BlockNumberKeeper blockNumberKeeper,
        DexSwapsKeeper dexSwapsKeeper,
        TokenTransferKeeper tokenTransferKeeper,
        CexDepositAddressKeeper cexDepositAddressKeeper,
        CexTraderInitializer cexTraderInitializer,
        TokenTransferChecker tokenTransferChecker)
    {
        _scopeFactory = scopeFactory;
        _dexTraderInitializer = dexTraderInitializer;
        _lockTaskKeeper = lockTaskKeeper;
        _blockNumberKeeper = blockNumberKeeper;
        _dexSwapsKeeper = dexSwapsKeeper;
        _tokenTransferKeeper = tokenTransferKeeper;
        _cexDepositAddressKeeper = cexDepositAddressKeeper;
        _cexTraderInitializer = cexTraderInitializer;
        _tokenTransferChecker = tokenTransferChecker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Task processor is starting...");

        Log.Debug($"The number of processors on this computer is {Environment.ProcessorCount}.");

        try
        {
            await SubscribeToUpdatesAsync();
            Log.Information("Started listening to tasks.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            throw;
        }
    }

    public async Task EstimatesAsync(EstimateResultsLegacyDto estimatedEvent)
    {
        await ProcessEstimateAsync(estimatedEvent);
    }

    private async Task ProcessEstimateAsync(EstimateResultsLegacyDto estimatedEvent)
    {
        var estimateLogger = Log.Logger;
        try
        {
            var processorStartProcessing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            CheckLatency(estimatedEvent, false);

            var guid = Guid.NewGuid();

            estimateLogger = CreateLogger(estimatedEvent, guid);

            // if (IS_DEBUG_ENVIRONMENT)
                // _soundNotifier.PlayInfoSoundAsync();

            var tasks = estimatedEvent.Tasks.Select((task) =>
                _tokenTransferChecker.SetTransfersForTask(task)).ToList();

            tasks = SelectValidTasks(tasks);

            // if (tasks.First().ExchangeOperations.First().NetworkId == Exchanges.BSC_EXCHANGE)
            // {
                tasks = tasks.GroupBy(task => task.BaseCurrency).Select(baseCurrencyTasks =>
                        baseCurrencyTasks.GroupBy(task => task.ExchangeOperations.First().TransactionSubmissionType)
                            .Select(submissionTypeTasks =>
                                submissionTypeTasks.MaxBy(task => task.EstimatedProfitInUsd)))
                    .SelectMany(tasks => tasks).ToList();
            // }
 
            var asyncTasks = tasks
                // .Where(task => task.BaseCurrency != USDC)
                // .Where(task => !(task.ExchangeOperations.First().NetworkId == SOL_EXCHANGE && task.ExchangeOperations.Any(x => x.ExchangeId is BINANCE)))
                .AsParallel()
                .Select((task) => HandleEstimatedTask(task, processorStartProcessing))
                .ToArray();

            if (asyncTasks.Length > 0)
            {
                estimateLogger.Information("debug 3367 {@Task}", tasks);
            }

            await Task.WhenAll(asyncTasks);
        }
        catch (ArbitrageException e)
        {
            estimateLogger.Error(e, "Failed to process task: {Reason}", e.Message);
        }
        catch (Exception e)
        {
            estimateLogger.Error("Failed to process task {Message} {StackTrace}", e.Message, e.StackTrace);
        }
    }

    private List<ArbitrageTask> SelectValidTasks(List<ArbitrageTask> tasks)
    {
        if (!tasks.Any(task => task.ArbitrageSource == ArbitrageSourceEnum.MevBlocker))
        {
            return tasks;
        }

        var tasksWithTransfers = tasks.Where(task => task.TransferredToTaskAmountRatio > 0).ToList();

        var bestTaskWithoutTransfers = tasks.Where(task => task.TransferredToTaskAmountRatio == 0)
            .MaxBy(task => task.EstimatedProfitInUsd);

        if (bestTaskWithoutTransfers != null)
        {
            var bestTaskExchangeTo = bestTaskWithoutTransfers.ExchangeOperations.Last().ExchangeId;

            var taskToSameExchangeToWithoutDexMultihop = tasks.FirstOrDefault(task =>
                task.ExchangeOperations.First().MarketOperations.Length == 1 &&
                task.ExchangeOperations.Last().ExchangeId == bestTaskExchangeTo
            );

            tasksWithTransfers.Add(taskToSameExchangeToWithoutDexMultihop ?? bestTaskWithoutTransfers);
        }


        return tasksWithTransfers;
    }


    private async Task HandleEstimatedTask(ArbitrageTask task, long processorStartProcessing)
    {
        var logger = CreateLogger(task);

        logger.Information("Received task {@Task}", task);

        if (task.ArbitrageType != ArbitrageTypeEnum.CexCex)
            task.FoundBlockNumber ??= _blockNumberKeeper.GetLastBlockNumber(task.ExchangeOperations.First().NetworkId);

        task.StartProcessingInProcessorTimestamp = processorStartProcessing;

        try
        {
            task.CustomProperties = new Dictionary<string, object>
            {
                { "orderbook", task.ExchangeOperations.Last().MarketOperations.First().OrderBook },
                { "dexPriceHistory", task.ExchangeOperations.First().PriceHistory },
                { "cexPriceHistory", task.ExchangeOperations.Last().PriceHistory }
            };
        }
        catch (Exception e)
        {
            logger.Warning(e, "Failed to add custom properties");
        }

        using var scope = _scopeFactory.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<ArbitrageTaskHandler>();

        await handler.HandleAsync(task, logger);
    }

    private void CheckLatency(EstimateResultsLegacyDto arbitrageEstimated, bool isTest)
    {
        var latency = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - arbitrageEstimated.PublishTimestamp;
        // _metrics.TaskProcessing.Info.UpdateEventLatencyData(latency, isTest);
        if (latency > MAX_ACCEPTABLE_LATENCY_MS)
            throw new ArbitrageException(0, $"Latency too big: {latency} ms");
    }


    private async Task SubscribeToUpdatesAsync()
    {
        await _cexTraderInitializer.InitializeAndLogTimeAsync();
        await _lockTaskKeeper.InitializeAndLogTimeAsync();
        await _cexDepositAddressKeeper.InitializeAndLogTimeAsync();
        await _dexTraderInitializer.InitializeAndLogTimeAsync();
        _blockNumberKeeper.SubscribeToUpdates();
        _dexSwapsKeeper.SubscribeToUpdates();
        await _tokenTransferKeeper.InitializeAndLogTimeAsync();
        await _smartConfigurationModule.InitializeAndLogTimeAsync();
    }
}