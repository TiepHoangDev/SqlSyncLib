using SqlSyncDbServiceLib.Helpers;

namespace SqlSyncDbServiceLib.BackupWorkers
{
    public class BackupWorkerState : WorkerStateVersionBase
    {
        public const string MinVersion_default = "no_min_version";
        public string MinVersion { get; set; } = MinVersion_default;

        public override string GetNextVersion<T>(string dir, string currentVersion)
        {
            if (currentVersion == null) return MinVersion;
            if (currentVersion.CompareTo(MinVersion) < 0) return MinVersion;
            return base.GetNextVersion<T>(dir, currentVersion);
        }

    }
}
