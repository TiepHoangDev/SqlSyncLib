namespace SqlSyncDbService.Workers.Interfaces
{
    public interface IFileBackup
    {
        Task<bool> BackupAsync(IWorkerConfig workerConfig, string pathFileZip);
    }
}
