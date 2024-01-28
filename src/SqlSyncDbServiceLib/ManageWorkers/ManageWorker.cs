using SqlSyncDbServiceLib.Interfaces;
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
            var cancellation = _tokenSource?.Token ?? throw new Exception("Please call RunAsync first");
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
            _taskCompletionSource = new TaskCompletionSource<bool>();
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _tokenSource.Token.Register(() => _taskCompletionSource.TrySetCanceled());
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

        public List<IWorker> GetWorkers(List<string> ids = null)
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
