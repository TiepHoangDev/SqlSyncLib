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
            var pathFile = backupConfig.GetPathFile(currentVersion, currentVersion);

            var success = await new FullBackupFileBackup().BackupAsync(backupConfig, pathFile);

            if (success)
            {
                if (backupState.MinVersion != null)
                {
                    // delete old version
                    var dirOld = Path.Combine(backupConfig.PathFolder, backupState.MinVersion);
                    var files = Directory.GetFiles(dirOld);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                    Directory.Delete(dirOld);
                }

                backupState.MinVersion = currentVersion;
                backupState.CurrentVersion = currentVersion;
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
            var success = await new LogBackupDatabase().CreateBackupAsync(sqlConnectString, pathFile);

            if (success)
            {
                backupState.CurrentVersion = currentVersion;
            }
            return success;
        }
    }
}
