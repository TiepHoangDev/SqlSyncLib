namespace SqlSyncDbService.Workers.Interfaces
{
    public interface IFileRestore
    {
        Task<bool> RestoreAsync(IWorkerConfig workerConfig, string pathFileZip);
    }
}
