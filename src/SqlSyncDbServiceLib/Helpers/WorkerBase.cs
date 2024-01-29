using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using SqlSyncDbServiceLib.Interfaces;
using SqlSyncDbServiceLib.LoggerWorkers;
using Microsoft.Extensions.Logging;

namespace SqlSyncDbServiceLib.Helpers
{
    public abstract class WorkerBase : IWorker
    {
        protected readonly ILogger logger;

        protected WorkerBase(ILogger logger)
        {
            this.logger = logger;
            Hooks.Add(new LoggerWorkerHook(logger));
        }

        public virtual List<IWorkerHook> Hooks { get; } = new List<IWorkerHook>();

        public virtual string Id => Config.Id;
        public abstract string Name { get; }
        public abstract IWorkerConfig Config { get; }
        public abstract IWorkerState State { get; }
        public virtual ILogger Logger => logger;
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
