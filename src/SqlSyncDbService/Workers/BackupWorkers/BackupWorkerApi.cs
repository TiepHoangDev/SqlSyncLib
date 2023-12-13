using FastQueryLib;
using SqlSyncDbService.Workers.Helpers;
using SqlSyncLib.LogicBase;
using System.Diagnostics;

namespace SqlSyncLib.Workers.BackupWorkers
{
    public class BackupWorkerApi
    {
        public virtual async Task<bool> BackupFull(BackupWorkerConfig backupConfig, BackupWorkerState backupState)
        {
            backupConfig.LastRunBackupFull = DateTime.Now;

            var sqlConnectString = backupConfig.SqlConnectString ?? throw new ArgumentNullException(backupConfig.SqlConnectString);
            var newVersion = VersionFactory.Instance.GetNewVersion();
            var success = true;

            //create dir
            var dir = Path.Combine(backupConfig.DirData, newVersion);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            //get path
            var pathFileFullBackUp = backupConfig.GetPathBackupFull(newVersion);
            var pathFileLogBackUp = backupConfig.GetPathFile(newVersion, newVersion);

            // set READ_ONLY => backup log => backup full => set READ_WRITE.
            var queryReadOnLy = $"ALTER DATABASE {backupConfig.DbName} SET READ_ONLY;";
            var queryReadWrite = $"ALTER DATABASE {backupConfig.DbName} SET READ_WRITE;";
            var master = SqlServerExecuterHelper.CreateConnection(sqlConnectString).NewOpenConnectToDatabase("master");
            try
            {
                await master.CreateFastQuery().WithQuery(queryReadOnLy).ExecuteNumberOfRowsAsync();
                await new LogBackupFileBackup().BackupAsync(backupConfig, pathFileLogBackUp);
                success = await new FullBackupFileBackup().BackupAsync(backupConfig, pathFileFullBackUp);
            }
            catch
            {
                throw;
            }
            finally
            {
                await master.CreateFastQuery().WithQuery(queryReadWrite).ExecuteNumberOfRowsAsync();
            }

            if (success)
            {
                //delete old version
                backupConfig.DeleteByMinVersion(backupState.MinVersion);

                //save state
                backupState.NextVersion = newVersion;
                backupConfig.SaveState(backupState);

                //update new version
                backupState.CurrentVersion = newVersion;
                backupState.MinVersion = newVersion;
                backupState.NextVersion = null;

                Debug.WriteLine("\tBackupFull success");
            }

            return success;
        }

        public virtual async Task<bool> BackupLog(BackupWorkerConfig backupConfig, BackupWorkerState backupState)
        {
            var dir = Path.Combine(backupConfig.DirData, backupState.MinVersion);
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
