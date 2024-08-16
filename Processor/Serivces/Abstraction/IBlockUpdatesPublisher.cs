using DexCexMevBot.Modules.Reserves.Contract;

namespace DexCexMevBot.Modules.Processor.Serivces.Abstraction
{
    public interface IBlockUpdatesPublisher
    {
        Task InitializeAsync();
        void SubscribeToNewBlocks(Func<NewBlockEvent, Task> newBlockHandler);
        void SubscribeToNewBlocks(Action<NewBlockEvent> newBlockHandler);
    }
}