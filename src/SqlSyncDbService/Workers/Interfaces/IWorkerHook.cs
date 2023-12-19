namespace SqlSyncDbService.Workers.Interfaces
{
    public interface IWorkerHook
    {
        string Name { get; }
        Task PostData(string? name, object data);
    }
}
