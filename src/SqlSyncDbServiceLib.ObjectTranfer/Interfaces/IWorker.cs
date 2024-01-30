using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.ObjectTranfer.Interfaces
{
    public interface IWorker : IDisposable
    {
        string Id { get; }
        string Name { get; }
        Task<bool> RunAsync(CancellationToken cancellationToken);
        List<IWorkerHook> Hooks { get; }
        IWorkerConfig Config { get; }
        IWorkerState State { get; }
        ISqlSyncDbServiceLibLogger Logger { get; }
    }
}
