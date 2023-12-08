namespace SqlSyncDbService.Workers.Interfaces
{
    public interface IWorkerHook
    {
        Task PostData(string? name, object data);
    }
}
