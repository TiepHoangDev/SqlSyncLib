using SqlSyncDbServiceLib.LoggerWorkers;
using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.Helpers
{
    public abstract class WorkerBase : IWorker
    {
        protected WorkerBase()
        {
            Hooks.Add(new ConsoleLogHook());
        }

        public virtual List<IWorkerHook> Hooks { get; } = new List<IWorkerHook>();

        public virtual string Id => Config.Id;
        public abstract string Name { get; }
        public abstract IWorkerConfig Config { get; }
        public abstract IWorkerState State { get; }
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

        protected virtual void DebugWriteLine(string msg)
        {
            Debug.WriteLine($"\t{msg}");
        }
    }
}
