using System.Collections.Concurrent;
using DexCexMevBot.Modules.Estimator.Exceptions;
using DexCexMevBot.Modules.Processor.Serivces.Abstract;
using DexCexMevBot.Modules.Processor.Serivces.Abstraction;
using DexCexMevBot.Modules.Reserves.Contract;
using DexCexMevBot.Modules.Reserves.Tokens.Models;
using Serilog;
using static DexCexMevBot.Constant.Exchanges;

namespace DexCexMevBot.Modules.Processor.Serivces;

public class TokenTransferKeeper : IInitializable
{
    private const string RPC_RESERVE_LISTENERS_QUEUE = "{0}_reserves_rpc_queue";

    private readonly Dictionary<string, long> CACHE_LIFETIME_IN_BLOCKS = new()
    {
        [ETH_EXCHANGE] = 100,
        // [BSC_EXCHANGE] = 100,
    };

    private readonly ConcurrentDictionary<string, ConcurrentQueue<TransferInfoDto>> _transfersQueues;
    private readonly CexDepositAddressKeeper _depositAddressKeeper;
    private readonly IBus _bus;
    private readonly IBlockUpdatesPublisher _blockUpdateService;

    public TokenTransferKeeper(
        CexDepositAddressKeeper depositAddressKeeper,
        IBus bus,
        IBlockUpdatesPublisher blockUpdateService)
    {
        _transfersQueues = new ConcurrentDictionary<string, ConcurrentQueue<TransferInfoDto>>();

        foreach (var networkId in EVM_DEX_EXCHANGES)
        {
            _transfersQueues.TryAdd(networkId, new ConcurrentQueue<TransferInfoDto>());
        }

        _depositAddressKeeper = depositAddressKeeper;
        _bus = bus;
        _blockUpdateService = blockUpdateService;
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(EVM_DEX_EXCHANGES.Select(GetAndSaveLastBlocksTransfers));
        SubscribeToTransferEvents();
    }

    private void SubscribeToTransferEvents()
    {
        _blockUpdateService.SubscribeToNewBlocks(OnTransferAsync);
    }

    private void OnTransferAsync(NewBlockEvent lastBlock)
    {
        try
        {
            var networkId = lastBlock.NetworkId;

            if (!EVM_DEX_EXCHANGES.Contains(networkId))
                return;

            var enrichedTransfers = EnrichTransfersWithExchangeIds(networkId, lastBlock.Transfers);

            SaveTransfers(networkId, enrichedTransfers, (int)lastBlock.BlockNumber);

            Log.Information("Block {NetworkId}-#{BlockNumber}. {CurrentCount} transfers saved. Total {Count} transfers",
                networkId,
                lastBlock.BlockNumber,
                enrichedTransfers.Length,
                _transfersQueues[networkId].Count);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error occured while processing block for saving token transfers {@block}", lastBlock);
        }
    }

    private async Task GetAndSaveLastBlocksTransfers(string networkId)
    {
        try
        {
            var request = new GetTransfersFromLastBlocksRequest
            {
                BlocksCount = CACHE_LIFETIME_IN_BLOCKS[networkId]
            };

            var watch = System.Diagnostics.Stopwatch.StartNew();
            var transfers = await GetTransfersFromReserves(request, networkId);
            watch.Stop();
            Log.Information("Received {NetworkId} transfers from reserves in {Elapsed} ms", networkId,
                watch.ElapsedMilliseconds);

            if (transfers == null)
                throw new ArbitrageException(0, $"Can`t receive last blocks transfers for {networkId}!");

            watch.Restart();
            foreach (var blockTransfers in transfers)
            {
                var enrichedTransfers = EnrichTransfersWithExchangeIds(networkId, blockTransfers.Value);
                SaveTransfers(networkId, enrichedTransfers, blockTransfers.Key);
            }

            watch.Stop();


            Log.Information("Saved {networkId} transfers for {blocksCount} blocks from reserves in {Elapsed} ms",
                networkId,
                transfers.Count,
                watch.ElapsedMilliseconds);
        }
        catch (ArbitrageException ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("Error getting last logs: {@ex}", ex);
        }
    }

    private async Task<Dictionary<int, TransferInfoDto[]>> GetTransfersFromReserves(
        GetTransfersFromLastBlocksRequest request, string networkId)
    {
        return (await _bus.RpcRequestAsync<GetTransfersFromLastBlocksRequest, GetTransfersFromLastBlocksResponse>(
                request,
                conf => conf.WithQueueName(GetReserveListenerQueue(networkId))))
            .MapResponse(success =>
                {
                    Log.Information("Received {NetworkId} transfers from reserves", networkId);

                    return success.Transfers;
                },
                error =>
                {
                    Log.Error("Error rpc response was received. Request: {@Request}. Error: {@Error}",
                        request,
                        error);

                    return null;
                });
    }

    private string GetReserveListenerQueue(string networkId)
    {
        return string.Format(RPC_RESERVE_LISTENERS_QUEUE, networkId);
    }

    private TransferInfoDto[] EnrichTransfersWithExchangeIds(string networkId, TransferInfoDto[] transfers)
    {
        var depositAddresses = _depositAddressKeeper.GetDepositAddresses(networkId);

        return transfers
            .Select(transfer =>
            {
                if (depositAddresses.TryGetValue(transfer.AddressTo.ToLower(), out var exchangeId))
                    transfer.ExchangeIdTo = exchangeId;

                return transfer;
            })
            .ToArray();
    }

    private void SaveTransfers(string networkId, TransferInfoDto[] transfers, int blockNumber)
    {
        if (transfers.Length == 0)
            return;

        foreach (var transfer in transfers)
        {
            transfer.BlockNumber = blockNumber;
            _transfersQueues[networkId].Enqueue(transfer);
        }

        while (_transfersQueues[networkId].TryDequeue(out var transfer))
        {
            var blockDifference = blockNumber - transfer.BlockNumber;

            if (blockDifference <= CACHE_LIFETIME_IN_BLOCKS[networkId])
            {
                break;
            }
        }
    }

    public TransferInfoDto[] GetTokenTransfersToExchange(string networkId, string tokenAddress, string exchangeId,
        int startBlock)
    {
        var transfers = _transfersQueues[networkId].ToArray();

        return transfers
            .Where(x =>
                x.BlockNumber >= startBlock &&
                x.ExchangeIdTo == exchangeId &&
                x.TokenAddress.ToLower() == tokenAddress.ToLower())
            .ToArray();
    }

    public IGrouping<string, TransferInfoDto>[] GetTransfersFromSwapsWithoutTransferToExchange(string networkId,
        string tokenAddress, int startBlock)
    {
        var transfers = _transfersQueues[networkId].Where(x => x.BlockNumber >= startBlock).ToArray();
        var transfersByTransactions = transfers.GroupBy(x => x.TxHash);
        var transactionsWithSwapWithoutTransferToExchange = new List<IGrouping<string, TransferInfoDto>>();

        foreach (var transaction in transfersByTransactions)
        {
            var orderedTransfers = transaction.OrderBy(x => x.LogIndex).ToArray();
            var firstTransfer = orderedTransfers.First();
            var lastTransfer = orderedTransfers.Last();
            var isSwapWithoutTransferToCex = (firstTransfer.AddressFrom == lastTransfer.AddressTo &&
                                              lastTransfer.TokenAddress == tokenAddress) ||
                                             (firstTransfer.AddressTo == lastTransfer.AddressFrom &&
                                              firstTransfer.TokenAddress == tokenAddress);

            if (isSwapWithoutTransferToCex)
                transactionsWithSwapWithoutTransferToExchange.Add(transaction);
        }

        return transactionsWithSwapWithoutTransferToExchange.ToArray();
    }
}