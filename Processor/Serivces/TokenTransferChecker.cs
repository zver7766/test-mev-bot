using DexCexMevBot.Modules.Estimator.Exceptions;
using DexCexMevBot.Modules.Estimator.Models.Enums;
using DexCexMevBot.Modules.Estimator.Models.Events;
using DexCexMevBot.Modules.Estimator.RouteEstimator;
using DexCexMevBot.Modules.Estimator.Utils;
using DexCexMevBot.Modules.Reserves.Blocks.Abstractions;
using DexCexMevBot.Utils;
using static System.Math;
using static DexCexMevBot.Constant.ErrorCodes;
using static DexCexMevBot.Constant.Exchanges;
using ILogger = Serilog.ILogger;

namespace DexCexMevBot.Modules.Processor.Serivces;

public class TokenTransferChecker
{
    private const decimal MAX_TRANSFERRED_TO_TASK_AMOUNT_RATIO = 0.3M;

    private readonly TokenTransferKeeper _tokenTransferKeeper;
    private readonly IBlockKeeperService _blockKeeperService;

    public TokenTransferChecker(
        TokenTransferKeeper tokenTransferKeeper,
        IBlockKeeperService blockKeeperService)
    {
        _tokenTransferKeeper = tokenTransferKeeper;
        _blockKeeperService = blockKeeperService;
    }

    public ArbitrageTaskLegacy SetTransfersForTask(ArbitrageTaskLegacy task)
    {
        var taskNetworkId = task.ExchangeOperations.Last().NetworkId;

        if (!task.ArbitrageType.IsDexCex())
            return task;

        // if (task.TargetCurrency == "GALA")
            // return task;

        // if (taskNetworkId == ARB_EXCHANGE || taskNetworkId == SOL_EXCHANGE || taskNetworkId == BASE_EXCHANGE)
            // return task;

        // if (task.IsMMTask)
            // return task;

        // if (task.IsAltcoinWithBalance)
            // return task;

        var logger = CustomLoggerFactory.CreateLogger(task);

        var dexId = task.ExchangeOperations.First().ExchangeId;
        var cexId = task.ExchangeOperations.Last().ExchangeId;
        var blockCount = CEX_DEPOSIT_BLOCK_CONFIRMATIONS_COUNT[dexId][cexId];
        var startBlock = (int)_blockKeeperService.GetLastBlockNumber() - blockCount;
        var taskTokenAmount = task.ExchangeOperations.Last().TradingAmountFrom;

        var transfers = _tokenTransferKeeper
            .GetTokenTransfersToExchange(taskNetworkId, task.TargetCurrencyAddress, cexId, startBlock);

        if (transfers.Any())
        {
            var transferredAmount = transfers.Sum(x => x.Amount);
            var transferredToTaskAmountRatio = transferredAmount / taskTokenAmount;
            task.CustomProperties["transfers"] = transfers;

            logger.Information("Transferred amount {TransferredAmount}, task amount {TaskAmount}, ratio {Ratio}",
                transferredAmount, taskTokenAmount, transferredToTaskAmountRatio);

            var recalculatedProfit =
                ReducedOrderBookCalculator.RecalculateProfitByReducedOrderBook(task, transferredAmount, logger);
            logger.Information("Recalculated profit {Profit} {Currency}", recalculatedProfit, task.BaseCurrency);
            task.RecalculatedProfitByTransferredAmount = recalculatedProfit;

            var latestTransferBlockNumber = transfers.Max(transfer => transfer.BlockNumber);
            task.LatestTransferBlockNumber = latestTransferBlockNumber;
            task.TransferredToTaskAmountRatio = transferredToTaskAmountRatio;
        }

        return task;
    }

    public void CheckIfSomeoneTransferredBefore(ArbitrageTaskLegacy task, ILogger logger)
    {
        // if (task.IsMMTask)
            // return;

        // this logic will work only for dex-cex
        if (ExchangeTypeResolver.IsCex(task.ExchangeOperations.First().ExchangeId))
            return;

        var cexId = task.ExchangeOperations.Last().ExchangeId;

        if (task.TransferredToTaskAmountRatio > MAX_TRANSFERRED_TO_TASK_AMOUNT_RATIO ||
            (task.RecalculatedProfitByTransferredAmount < 0 && !LIQUID_CEX_EXCHANGES.Contains(cexId)))
        {
            var taskTokenAmount = task.ExchangeOperations.Last().TradingAmountFrom;
            var transferredAmount = taskTokenAmount * task.TransferredToTaskAmountRatio;

            throw new ArbitrageException(
                SOMEONE_TRANSFERRED_BEFORE,
                $"Token {task.TargetCurrency} already transferred to {cexId}.\n" +
                $"Transferred amount: {Round(transferredAmount, 4)}\n" +
                $"TaskAmount: {Round(taskTokenAmount, 4)}\n" +
                $"Ratio: {Round(task.TransferredToTaskAmountRatio, 4)}\n" +
                $"Last transfer block: {task.LatestTransferBlockNumber}");
        }
    }
}