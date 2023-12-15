using FastQueryLib;
using SqlSyncDbService.Workers.Helpers;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;

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
            bool isReadOnly = await SqlServerExecuterHelper.CreateConnection(sqlConnectString)
                .CreateFastQuery().IsReadOnlyAsync();
            try
            {
                if (isReadOnly)
                {
                    using var master = SqlServerExecuterHelper.CreateConnection(sqlConnectString);
                    var _sourceToken = new CancellationTokenSource(TimeSpan.FromMinutes(3));
                    while (!_sourceToken.IsCancellationRequested)
                    {
                        var connecttionOnWorking = await master.CreateFastQuery().CountNumberConnecttionOnDatabase();
                        if (connecttionOnWorking <= 1)
                        {
                            //set Backup log
                            await new LogBackupFileBackup(backupState).BackupAsync(backupConfig, pathFileLogBackUp);

                            //set Read-OnLy
                            await master.CreateFastQuery().SetDatabaseReadOnly(true);
                            break;
                        }
                        Debug.WriteLine($"Database {backupConfig.DbName} have {connecttionOnWorking} connect are working. Wait 100s for all done. Max wait 3m.");
                        await Task.Delay(100, _sourceToken.Token);
                    }
                }

                //set Backup full
                success = await new FullBackupFileBackup(backupState).BackupAsync(backupConfig, pathFileFullBackUp);
            }
            catch
            {
                throw;
            }
            finally
            {
                //set Read-Write
                if (!isReadOnly)
                {
                    await SqlServerExecuterHelper.CreateConnection(sqlConnectString).CreateFastQuery().SetDatabaseReadOnly(false);
                }
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
            var success = await new LogBackupFileBackup(backupState).BackupAsync(backupConfig, pathFile);

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
