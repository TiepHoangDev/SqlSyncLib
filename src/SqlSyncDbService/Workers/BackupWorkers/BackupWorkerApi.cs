using SqlSyncDbService.Workers.Helpers;
using SqlSyncLib.LogicBase;
using System.Diagnostics;

namespace SqlSyncLib.Workers.BackupWorkers
{
    public class BackupWorkerApi
    {
        public virtual async Task<bool> BackupFull(BackupWorkerConfig backupConfig, BackupWorkerState backupState)
        {
            backupConfig.LastRun = DateTime.Now;

            var currentVersion = VersionFactory.Instance.GetNewVersion();
            var dir = Path.Combine(backupConfig.PathFolder, currentVersion);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var pathFile = backupConfig.GetPathBackupFull(currentVersion);

            var success = await new FullBackupFileBackup().BackupAsync(backupConfig, pathFile);

            if (success)
            {
                //delete old version
                backupConfig.DeleteMinVersion(backupState.MinVersion);

                //save old version
                backupState.NextVersion = currentVersion;
                backupConfig.SaveState(backupState);

                //update new version
                backupState.MinVersion = currentVersion;
                backupState.CurrentVersion = currentVersion;
                backupState.NextVersion = null;
            }

            return success;
        }

        public virtual async Task<bool> BackupLog(BackupWorkerConfig backupConfig, BackupWorkerState backupState)
        {
            backupConfig.LastRun = DateTime.Now;

            var sqlConnectString = backupConfig.SqlConnectString ?? throw new ArgumentNullException(nameof(backupConfig.SqlConnectString));
            var dir = Path.Combine(backupConfig.PathFolder, backupState.MinVersion);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var currentVersion = VersionFactory.Instance.GetNewVersion();
            var pathFile = backupConfig.GetPathFile(backupState.MinVersion, currentVersion);
            var success = await new LogBackupFileBackup().BackupAsync(backupConfig, pathFile);

            if (success)
            {
                //save old version
                backupState.NextVersion = currentVersion;
                backupConfig.SaveState(backupState);

                //update new version
                backupState.CurrentVersion = currentVersion;
                backupState.NextVersion = null;
            }
            return success;
        }
    }
}
