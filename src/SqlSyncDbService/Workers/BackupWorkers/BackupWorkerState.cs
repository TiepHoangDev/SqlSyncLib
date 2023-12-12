using SqlSyncDbService.Workers.Helpers;
using SqlSyncDbService.Workers.Interfaces;

namespace SqlSyncLib.Workers.BackupWorkers
{
    public record BackupWorkerState : WorkerStateBase
    {
        public const string MinVersion_default = "no_min_version";

        public string MinVersion { get; set; } = MinVersion_default;
        public string? CurrentVersion { get; set; }
        public string? NextVersion { get; set; }
        public bool IsNoMinVersion => MinVersion == MinVersion_default;
    }
}
