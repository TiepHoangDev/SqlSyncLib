using SqlSyncDbServiceLib.BackupWorkers;
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
        private TaskCompletionSource<bool> _taskCompletionSource = null;
        private CancellationTokenSource _tokenSource = null;
        public ILoaderConfig LoaderConfig { get; private set; }

        public ManageWorker(ILoaderConfig loaderConfig)
        {
            _taskCompletionSource = new TaskCompletionSource<bool>();
            _tokenSource = new CancellationTokenSource();
            _tokenSource.Token.Register(() => _taskCompletionSource.TrySetCanceled());
            LoaderConfig = loaderConfig;
            Init();
        }

        private void Init()
        {
            foreach (var backupWorkerConfig in LoaderConfig.BackupWorkerConfigs)
            {
                AddWorker(new BackupWorker
                {
                    BackupConfig = backupWorkerConfig
                });
            }
            foreach (var restoreWorkerConfig in LoaderConfig.RestoreWorkerConfigs)
            {
                AddWorker(new RestoreWorker
                {
                    RestoreConfig = restoreWorkerConfig
                });
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
            var cancellation = _tokenSource.Token;
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            var id = worker?.Id ?? throw new Exception("empty-id");
            if (Workers.ContainsKey(id))
            {
                throw new Exception($"worker-already-exist: {id}");
            }
            var workerItem = new ManageWorkerItem(worker, tokenSource);
            Workers.Add(id, workerItem);
            worker.RunAsync(tokenSource.Token);
            return true;
        }

        public async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
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
            return Workers.Values
                .Where(q => ids?.Any() != true || ids.Contains(q.Worker.Id))
                .Select(q => q.Worker).ToList();
        }

        public bool RemoveWorker(string id)
        {
            return RemoveWorker(q => q.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
