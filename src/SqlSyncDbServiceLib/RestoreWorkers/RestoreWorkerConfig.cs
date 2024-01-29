using System.IO;

namespace SqlSyncDbServiceLib.RestoreWorkers
{
    public class RestoreWorkerConfig : WorkerConfigBase
    {
        public string IdBackupWorker { get; set; }
        public string BackupAddress { get; set; }
        public int MaxFileDownload { get; set; } = 50;

        public override void OnUpdateSqlConnectionString(string newValue, string oldValue)
        {
            base.OnUpdateSqlConnectionString(newValue, oldValue);
            DirData = Path.Combine(DirRoot, "restore");
        }

        public RestoreWorkerState GetStateByVersion(string version)
            => base.GetStateByVersion<RestoreWorkerState>(version);
    }
}
