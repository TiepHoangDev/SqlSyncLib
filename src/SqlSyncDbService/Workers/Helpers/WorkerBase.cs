using SqlSyncDbService.Workers.Interfaces;
using SqlSyncDbService.Workers.LoggerWorkers;

namespace SqlSyncDbService.Workers.Helpers
{
    public abstract class WorkerBase : IWorker
    {
        public virtual List<IWorkerHook> Hooks { get; } = new List<IWorkerHook> {
            new LoggerWorkerHook()
        };

        public virtual string Id => Name;
        public abstract string Name { get; }
        public abstract IWorkerConfig Config { get; }
        public abstract Task<bool> RunAsync(CancellationToken cancellationToken);

        public virtual void Dispose()
        {

        }

        protected virtual async void CallHookAsync(string name, object data)
        {
            foreach (var item in Hooks)
            {
                await item.PostData(name, data);
            }
        }
    }
}
