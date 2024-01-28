using FastQueryLib;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Data.SqlClient;
using SqlSyncDbServiceLib.Helpers;

namespace SqlSyncDbServiceLib.BackupWorkers
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

            // set single user => backup log => backup full => set mutil user.
            using (var dbConnection = SqlServerExecuterHelper.CreateConnection(sqlConnectString))
            {
                dbConnection.Open();
                await dbConnection.CreateFastQuery().UseSingleUserModeAsync(async fastquery =>
                {
                    await new LogBackupFileBackup(backupState).BackupAsync(dbConnection, pathFileLogBackUp);
                    success = await new FullBackupFileBackup(backupState).BackupAsync(dbConnection, pathFileFullBackUp);
                });
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
                backupConfig.SaveState(backupState);

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
            using (var dbConnection = new SqlConnection(backupConfig.SqlConnectString))
            {
                var success = await new LogBackupFileBackup(backupState).BackupAsync(dbConnection, pathFile);

                if (success)
                {
                    //save old version
                    backupState.NextVersion = currentVersion;
                    backupConfig.SaveState(backupState);

                    //update new version
                    backupState.CurrentVersion = currentVersion;
                    backupState.NextVersion = null;
                    backupConfig.SaveState(backupState);
                }
                return success;
            }
        }
    }
}
