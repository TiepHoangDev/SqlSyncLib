namespace SqlSyncDbService.Workers.Interfaces
{
    public interface IWorker : IDisposable
    {
        string Id { get; }
        string Name { get; }
        Task<bool> RunAsync(CancellationToken cancellationToken);
        List<IWorkerHook> Hooks { get; }
        IWorkerConfig Config { get; }
    }
}
