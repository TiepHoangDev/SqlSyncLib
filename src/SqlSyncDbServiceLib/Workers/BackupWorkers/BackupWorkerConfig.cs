using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using SqlSyncDbService.Workers.Helpers;
using SqlSyncDbServiceLib.Workers.Interfaces;
using System.Diagnostics;
using System.Text.Json;

namespace SqlSyncDbServiceLib.Workers.BackupWorkers
{
    public class BackupWorkerConfig : WorkerConfigBase
    {
        public TimeOnly ResetAtTime { get; set; } = new TimeOnly(4, 0, 0);
        public DayOfWeek? ResetAtDay { get; set; } = DayOfWeek.Saturday;
        public DateTime? LastRunBackupFull { get; set; }

        public override void OnUpdateSqlConnectionString(string newValue, string oldValue)
        {
            base.OnUpdateSqlConnectionString(newValue, oldValue);
            DirData = Path.Combine(DirRoot, "backup");
        }

        public bool IsReset(DateTime now)
        {
            if (ResetAtDay.HasValue && now.DayOfWeek != ResetAtDay) return false;
            if (now.TimeOfDay - ResetAtTime.ToTimeSpan() > DelayTime) return false;
            return true;
        }

        public bool IsExistBackupFull(string minVersion)
        {
            return File.Exists(GetPathBackupFull(minVersion));
        }

        public string GetPathBackupFull(string version) => GetPathFile(version, "full");

        public string GetPathFile(string minVersion, string version)
        {
            var path = Path.Combine(DirData, minVersion, $"{DbName}.{version}.syncdb");
            return Path.GetFullPath(path);
        }

        public void DeleteByMinVersion(string minVersion)
        {
            var path = GetPathBackupFull(minVersion);
            var dir = Path.GetDirectoryName(path);
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        public BackupWorkerState? GetStateByVersion(string version)
            => GetStateByVersion<BackupWorkerState>(version);

    }
}
