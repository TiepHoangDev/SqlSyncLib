using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSyncLib.Workers
{
    public interface IWorker
    {
        string Name { get; }
        Task<bool> RunAsync(CancellationToken cancellationToken);
        List<IWorkerHook> Hooks { get; }
        IWorkerConfig Config { get; }
        IWorkerApi Api { get; }
        IWorkerState State { get; }
    }

    public interface IWorkerState
    {
        string DbId { get; }
    }
    public interface IWorkerApi { }
    public interface IWorkerInput { }
    public interface IWorkerHook { }
    public interface IWorkerConfig
    {
        TimeSpan DelayTime { get; }
    }
}
