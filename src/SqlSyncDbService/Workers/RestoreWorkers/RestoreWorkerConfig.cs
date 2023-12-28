using Microsoft.Data.SqlClient;
using SqlSyncDbService.Workers.Helpers;
using SqlSyncDbService.Workers.Interfaces;
using SqlSyncDbService.Workers.BackupWorkers;
using System.Xml.Linq;

namespace SqlSyncDbService.Workers.RestoreWorkers
{
    public class RestoreWorkerConfig : WorkerConfigBase
    {
        public string? IdBackupWorker { get; set; }
        public string? BackupAddress { get; set; }
        public int MaxFileDownload { get; set; } = 50;

        public override void OnUpdateSqlConnectionString(string? newValue, string? oldValue)
        {
            base.OnUpdateSqlConnectionString(newValue, oldValue);
            DirData = Path.Combine(DirRoot, "restore");
        }

        public RestoreWorkerState? GetStateByVersion(string version)
            => base.GetStateByVersion<RestoreWorkerState>(version);
    }
}
