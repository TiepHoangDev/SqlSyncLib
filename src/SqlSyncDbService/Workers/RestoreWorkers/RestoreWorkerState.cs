using SqlSyncDbService.Workers.Helpers;
using SqlSyncDbService.Workers.Interfaces;

namespace SqlSyncDbService.Workers.RestoreWorkers
{
    public record RestoreWorkerState : WorkerStateVersionBase
    {
        public string? DownloadedVersion { get; set; }
    }
}
