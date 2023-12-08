namespace SqlSyncDbService.Workers.Interfaces
{
    public interface IManageWorker : IDisposable, IExportApi
    {
        bool AddWorker(IWorker worker);
        bool RemoveWorker(Func<IWorker, bool> workerSelector);
        Task<bool> RunAsync(CancellationToken cancellationToken);
    }
}
