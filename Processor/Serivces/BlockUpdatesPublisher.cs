using System.Collections.Concurrent;
using DexCexMevBot.Constant;
using DexCexMevBot.Modules.Processor.Serivces.Abstraction;
using DexCexMevBot.Modules.Reserves.Contract;
using Serilog;

namespace DexCexMevBot.Modules.Processor.Serivces
{
    public class BlockUpdatesPublisher : IBlockUpdatesPublisher
    {
        private readonly ConcurrentBag<Func<NewBlockEvent, Task>> _onUpdateAsyncHandler = [];
        private readonly ConcurrentBag<Action<NewBlockEvent>> _onUpdateSyncHandler = [];
        private bool _isInitialized;

        public BlockUpdatesPublisher()
        {
        }

        public async Task InitializeAsync()
        {
            // if (_isInitialized)
            // {
                // return;
            // }

            // while (_isInitialized)
            // {
                // _isInitialized = await NewBlocksAsync();
            // }

            // _isInitialized = true;
            // await NewBlocksAsync();
        }

        public void SubscribeToNewBlocks(Func<NewBlockEvent, Task> newBlockHandler)
        {
            _onUpdateAsyncHandler.Add(newBlockHandler);
        }

        public void SubscribeToNewBlocks(Action<NewBlockEvent> newBlockHandler)
        {
            _onUpdateSyncHandler.Add(newBlockHandler);
        }

        public async Task NewBlocksAsync(NewBlockEvent lastBlock)
        {
            if (!Exchanges.DEX_EXCHANGES.Contains(lastBlock.NetworkId))
                return;

            Log.Debug($"Block {lastBlock.NetworkId}-#{lastBlock.BlockNumber} started to be handled.");
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var tasks = _onUpdateAsyncHandler.Select(onBlockUpdateEvent => onBlockUpdateEvent(lastBlock)).ToList();
            _onUpdateSyncHandler.ToList().ForEach(syncTask => syncTask(lastBlock));
            await Task.WhenAll(tasks);

            watch.Stop();
            Log.Debug($"Block {lastBlock.NetworkId}-#{lastBlock.BlockNumber} was handled by {tasks.Count + _onUpdateSyncHandler.Count} handlers with {watch.ElapsedMilliseconds}ms.");
        }
    }
}