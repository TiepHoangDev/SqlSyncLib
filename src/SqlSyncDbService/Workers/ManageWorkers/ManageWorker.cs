using Azure.Core;
using SqlSyncDbService.Workers.Interfaces;
using System.Reflection;
using System.Text.Json;

namespace SqlSyncDbService.Workers.ManageWorkers
{
    public class ManageWorker : IManageWorker
    {
        private readonly Dictionary<string, ManageWorkerItem> Workers = new Dictionary<string, ManageWorkerItem>();
        private TaskCompletionSource<bool>? _taskCompletionSource;
        private CancellationTokenSource? _tokenSource;

        record ManageWorkerItem(IWorker worker, CancellationTokenSource TokenSource) : IDisposable
        {
            public void Dispose()
            {
                if (TokenSource.Token.CanBeCanceled) TokenSource.Cancel();
            }
        }

        public bool RemoveWorker(Func<IWorker, bool> workerSelector)
        {
            var workers = Workers.Values.Where(q => workerSelector.Invoke(q.worker)).ToList();
            foreach (var item in workers)
            {
                item.Dispose();
                Workers.Remove(item.worker.Id);
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
        }

        public List<IWorker> GetWorkers(List<string>? ids = null)
        {
            return Workers.Values
                .Where(q => ids?.Any() != true || ids.Contains(q.worker.Id))
                .Select(q => q.worker).ToList();
        }

        public bool RemoveWorker(string id)
        {
            return RemoveWorker(q => q.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        //public virtual void MapEndPoint(MapEndPointInput input)
        //{
        //    //AddWorker
        //    var type = typeof(IWorker);
        //    var classWorkers = AppDomain.CurrentDomain.GetAssemblies()
        //        .SelectMany(q => q.GetTypes())
        //        .Where(q => type.IsAssignableFrom(q))
        //        .Where(q => q.IsClass && !q.IsAbstract)
        //        .ToList();
        //    foreach (var classWorker in classWorkers)
        //    {
        //        var name = classWorker.Name;
        //        var endpoint = $"/AddWorker/{name}";
        //        input.routeBuilder.MapPost(endpoint, (string workerJson) =>
        //        {
        //            var data = JsonSerializer.Deserialize(workerJson, classWorker);
        //            var worker = data as IWorker ?? throw new Exception($"Can not parse data to type {name}");
        //            return AddWorker(worker);
        //        }).WithTags(nameof(ManageWorker));
        //    }
        //}
    }
}
