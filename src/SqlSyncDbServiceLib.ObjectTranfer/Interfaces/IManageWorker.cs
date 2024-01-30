using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.ObjectTranfer.Interfaces
{
    public interface IManageWorker : IDisposable
    {
        ILoaderConfig LoaderConfig { get; }
        List<IWorker> GetWorkers(List<string> ids);
        bool AddWorker(IWorker worker);
        bool RemoveWorker(Func<IWorker, bool> workerSelector);
        Task<bool> RunAsync(CancellationToken cancellationToken);
    }
}
