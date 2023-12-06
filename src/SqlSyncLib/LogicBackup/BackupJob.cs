using FastQueryLib;
using Microsoft.Data.SqlClient;
using SqlSyncLib.Interfaces;
using SqlSyncLib.LogicBase;
using System.Diagnostics;

namespace SqlSyncLib.LogicBackup
{
    public class BackupJob : BaseJob, IBackupJob
    {
        public BackupJob(BackupJobSetting syncSetting)
        {
            Setting = syncSetting;
        }

        public readonly BackupJobSetting Setting;

        public async Task<IItemSync> CreateBackupFullAsync()
        {
            var version = VersionFactory.GetNewVersion();
            var file = Setting.CreatePathFileBackupFull(version);
            var query = $" BACKUP DATABASE [{Setting.DbName}] TO DISK='{file}' WITH FORMAT; ";
            await ExecuteBackupQuery(query);

            var metadata = new SqlSyncMetadata(version, version);
            return new BackupFullItemSync(metadata);
        }

        public async Task<IItemSync> CreateBackupLogAsync()
        {
            var minVer = Setting.GetLatestBackupFullVersion();
            if (minVer == null)
            {
                var backupFull = await CreateBackupFullAsync();
                minVer = backupFull.Metadata.MinVersion;
                Debug.WriteLine($"There are not exist backup full, created backup full success on version = {minVer}");
            }

            var version = VersionFactory.GetNewVersion();
            var file = Setting.CreatePathFileBackupLog(version);
            var query = $" BACKUP LOG [{Setting.DbName}] TO DISK='{file}' WITH FORMAT; ";
            await ExecuteBackupQuery(query);

            var metadata = new SqlSyncMetadata(version, minVer);
            return new BackupLogItemSync(metadata);
        }

        private async Task ExecuteBackupQuery(string query)
        {
            using var conn = new SqlConnection(Setting.connectString);
            if (!await conn.CheckDatabaseExistsAsync(Setting.DbName))
            {
                throw new Exception($"Not exit database [{Setting.DbName}].");
            }

            using var master = conn.NewOpenConnectToDatabase("master");
            using var backup = await master.CreateFastQuery().WithQuery(query).ExecuteNumberOfRowsAsync();
        }

    }
}
