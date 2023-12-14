namespace SqlSyncDbService.Workers.RestoreWorkers
{
    public interface IRestoreDownload
    {
        Task<string?> DownloadFileAsync(RestoreWorkerConfig RestoreConfig, RestoreWorkerState RestoreState, CancellationToken cancellationToken);
    }
}
