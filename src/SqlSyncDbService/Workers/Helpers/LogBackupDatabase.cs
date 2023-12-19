using FastQueryLib;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Data.SqlClient;

namespace SqlSyncDbService.Workers.Helpers
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
            using var faster = sqlConnection.CreateFastQuery();
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
}
