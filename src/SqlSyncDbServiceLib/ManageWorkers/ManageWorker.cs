using SqlSyncDbServiceLib.BackupWorkers;
using SqlSyncDbServiceLib.LoggerWorkers;
using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using SqlSyncDbServiceLib.RestoreWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.ManageWorkers
{
    public class ManageWorker : IManageWorker
    {
        private readonly Dictionary<string, ManageWorkerItem> Workers = new Dictionary<string, ManageWorkerItem>();
        private readonly IDbServiceLibLogger Logger = null;
        private readonly TaskCompletionSource<bool> _taskCompletionSource = null;
        private readonly CancellationTokenSource _tokenSource = null;
        public ILoaderConfig LoaderConfig { get; private set; }

        public ManageWorker(ILoaderConfig loaderConfig, IDbServiceLibLogger dbServiceLibLogger)
        {
            Logger = dbServiceLibLogger;
            _taskCompletionSource = new TaskCompletionSource<bool>();
            _tokenSource = new CancellationTokenSource();
            _tokenSource.Token.Register(() => _taskCompletionSource.TrySetCanceled());
            LoaderConfig = loaderConfig;
        }

        private void ReloadWorders()
        {
            var workers = new List<IWorker>();
            if (LoaderConfig == null)
            {
                Logger?.Log($"[WARNING] {LoaderConfig} is null. It need to save config works to load after restart!");
                return;
            }

            workers.AddRange(LoaderConfig.BackupWorkerConfigs.Select(x => new BackupWorker { BackupConfig = x }));
            workers.AddRange(LoaderConfig.RestoreWorkerConfigs.Select(x => new RestoreWorker { RestoreConfig = x }));
            foreach (var worker in workers)
            {
                try
                {
                    AddWorker(worker);
                }
                catch (Exception ex)
                {
                    Logger?.Log(ex);
                }
            }
        }

        class ManageWorkerItem : IDisposable
        {
            public IWorker Worker { get; set; }
            public CancellationTokenSource TokenSource { get; set; }

            public ManageWorkerItem(IWorker worker, CancellationTokenSource tokenSource)
            {
                Worker = worker;
                TokenSource = tokenSource;
            }

            public void Dispose()
            {
                if (TokenSource.Token.CanBeCanceled) TokenSource.Cancel();
                GC.SuppressFinalize(this);
            }
        }

        public bool RemoveWorker(Func<IWorker, bool> workerSelector)
        {
            var workers = Workers.Values.Where(q => workerSelector.Invoke(q.Worker)).ToList();
            foreach (var item in workers)
            {
                item.Dispose();
                Workers.Remove(item.Worker.Id);
            }
            return workers.Any();
        }

        public bool AddWorker(IWorker worker)
        {
            //check
            var id = worker?.Id ?? throw new Exception("empty-id");
            if (Workers.ContainsKey(id))
            {
                throw new Exception($"worker-already-exist: {id}");
            }

            //add hook default to write log
            worker.Hooks.Add(new FailedLoggerWorkerHook(Logger));

            //add worker
            var cancellation = _tokenSource.Token;
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            var workerItem = new ManageWorkerItem(worker, tokenSource);
            Workers.Add(id, workerItem);

            //run
            worker.RunAsync(tokenSource.Token);
            Logger?.Log($"AddWorker {worker.Id} success");

            return true;
        }

        public async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            ReloadWorders();
            cancellationToken.Register(() => _tokenSource.Cancel());
            return await _taskCompletionSource.Task;
        }

        public void Dispose()
        {
            foreach (var item in Workers)
            {
                item.Value.Dispose();
            }
            Workers.Clear();

            _taskCompletionSource?.SetResult(true);

            GC.SuppressFinalize(this);
        }

        public List<IWorker> GetWorkers(List<string> ids)
        {
            var data = Workers.Values.Select(q => q.Worker);
            if (ids?.Any() ?? false)
            {
                data = data.Where(q => ids.Contains(q.Id));
            }
            return data.ToList();
        }

        public bool RemoveWorker(string id)
        {
            return RemoveWorker(q => q.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
    }

}
