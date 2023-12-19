using SqlSyncDbService.Workers.Interfaces;
using SqlSyncDbService.Workers.LoggerWorkers;
using System.Diagnostics;

namespace SqlSyncDbService.Workers.Helpers
{
    public abstract class WorkerBase : IWorker
    {
        protected readonly ILogger? logger;

        protected WorkerBase(ILogger? logger)
        {
            this.logger = logger;
            Hooks.Add(new LoggerWorkerHook(logger));
        }

        public virtual List<IWorkerHook> Hooks { get; } = new();

        public virtual string Id => Config.Id;
        public abstract string Name { get; }
        public abstract IWorkerConfig Config { get; }
        public abstract IWorkerState State { get; }
        public virtual ILogger? Logger => logger;
        public abstract Task<bool> RunAsync(CancellationToken cancellationToken);

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        protected virtual async void CallHookAsync(string name, object data)
        {
            foreach (var item in Hooks)
            {
                await item.PostData(name, data);
            }
        }

        protected virtual void WriteLine(string msg)
        {
            Debug.WriteLine($"\t{msg}");
        }
    }
}
