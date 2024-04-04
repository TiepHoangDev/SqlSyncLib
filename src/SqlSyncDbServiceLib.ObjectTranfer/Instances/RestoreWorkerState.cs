using SqlSyncDbServiceLib.Helpers;

namespace SqlSyncDbServiceLib.ObjectTranfer.Instances
{
    public class RestoreWorkerState : WorkerStateVersionBase
    {
        public string DownloadedVersion { get; set; }
    }
}
