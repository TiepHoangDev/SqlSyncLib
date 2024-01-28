using SqlSyncDbServiceLib.Helpers;

namespace SqlSyncDbServiceLib.RestoreWorkers
{
    public class RestoreWorkerState : WorkerStateVersionBase
    {
        public string DownloadedVersion { get; set; }
    }
}
