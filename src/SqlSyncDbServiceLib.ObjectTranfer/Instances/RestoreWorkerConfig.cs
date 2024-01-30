using System.IO;

namespace SqlSyncDbServiceLib.ObjectTranfer.Instances
{
    public class RestoreWorkerConfig : WorkerConfigBase
    {
        public string IdBackupWorker { get; set; }
        public string BackupAddress { get; set; }
        public int MaxFileDownload { get; set; } = 50;

        public static RestoreWorkerConfig Create(string SqlConnectString, string BackupAddress, string IdBackupWorker)
        {
            return new RestoreWorkerConfig
            {
                BackupAddress = BackupAddress,
                IdBackupWorker = IdBackupWorker,
                SqlConnectString = SqlConnectString,
            };
        }

        public override void OnUpdateSqlConnectionString(string newValue, string oldValue)
        {
            base.OnUpdateSqlConnectionString(newValue, oldValue);
            DirData = Path.Combine(DirRoot, "restore");
        }

        public RestoreWorkerState GetStateByVersion(string version)
            => base.GetStateByVersion<RestoreWorkerState>(version);
    }
}
