using SqlSyncDbServiceLib.Helpers;
using System;

namespace SqlSyncDbServiceLib.ObjectTranfer.Instances
{
    public class BackupWorkerState : WorkerStateVersionBase
    {
        public const string MinVersion_default = "no_min_version";
        public string MinVersion { get; set; } = MinVersion_default;
        public DateTime? LastRunBackupFull { get; set; }

        public override string GetNextVersion<T>(string dir, string currentVersion)
        {
            if (currentVersion == null) return MinVersion;
            if (currentVersion.CompareTo(MinVersion) < 0) return MinVersion;
            return base.GetNextVersion<T>(dir, currentVersion);
        }

    }
}
