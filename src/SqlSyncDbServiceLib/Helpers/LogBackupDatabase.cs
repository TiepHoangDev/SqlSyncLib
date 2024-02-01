using FastQueryLib;
using System.Data.SqlClient;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.Helpers
{
    public class LogBackupDatabase : BackupDatabaseBase
    {
        protected override string GetQueryBackup(string dbName, string pathFile)
        {
            var query = $"USE master; BACKUP LOG [{dbName}] TO DISK='{pathFile}' WITH FORMAT; Use [{dbName}]; ";
            return query;
        }

        protected override string GetQueryRestore(string dbName, string pathFile, string minVersion)
        {
            var standby = GetFileStandBy(dbName, pathFile, minVersion);
            var query = $"USE master; RESTORE LOG [{dbName}] FROM DISK='{pathFile}' WITH STANDBY='{standby}'; USE [{dbName}]; ";
            return query;
        }

        public override async Task<bool> RestoreBackupAsync(SqlConnection sqlConnection, string pathFile, string minVersion)
        {
            var dbName = sqlConnection.Database;
            using (var faster = sqlConnection.CreateFastQuery())
            {
                await faster.UseSingleUserModeAsync(async f =>
                {
                    var fullPath = Path.GetFullPath(pathFile);
                    var queryRestore = GetQueryRestore(dbName, fullPath, minVersion);
                    await sqlConnection.CreateFastQuery()
                       .WithQuery(queryRestore)
                       .ExecuteNonQueryAsync();
                });
                return true;
            }
        }

        public override async Task<bool> CreateBackupAsync(SqlConnection sqlConnection, string pathFile)
        {
            var dbName = sqlConnection.Database;

            //check database has backup full before
            var hasBackupFull = await sqlConnection.CreateFastQuery()
                .ThrowIfIsSystemDb()
                .WithQuery($"USE msdb; SELECT TOP 1 1 FROM backupset WHERE database_name = '{dbName}'; USE {dbName}; ")
                .ExecuteScalarAsync<int?>();
            if (hasBackupFull != 1)
            {
                throw new Exception("Database must backup full before Backup log.");
            }

            return await base.CreateBackupAsync(sqlConnection, pathFile);
        }
    }
}
