using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.Interfaces
{
    public interface IManageWorker : IDisposable
    {
        List<IWorker> GetWorkers(List<string> ids);
        bool AddWorker(IWorker worker);
        bool RemoveWorker(Func<IWorker, bool> workerSelector);
        Task<bool> RunAsync(CancellationToken cancellationToken);
    }
}
