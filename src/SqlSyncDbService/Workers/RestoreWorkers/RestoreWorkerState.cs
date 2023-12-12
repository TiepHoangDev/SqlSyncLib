using SqlSyncDbService.Workers.Helpers;
using SqlSyncDbService.Workers.Interfaces;

namespace SqlSyncDbService.Workers.RestoreWorkers
{
    public record RestoreWorkerState : WorkerStateBase
    {
        public string? CurrentVersion { get; set; }

    }
}
