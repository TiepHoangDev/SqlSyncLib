using FastQueryLib;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using Microsoft.Net.Http.Headers;
using SqlSyncDbService.Workers.Interfaces;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace SqlSyncDbService.Workers.Helpers
{
    public abstract class BackupDatabaseBase
    {
        public virtual async Task<bool> CreateBackupAsync(SqlConnection sqlConnection, string pathFile)
        {
            var dbName = sqlConnection.Database;

            //check database has backup full before
            //USE msdb; SELECT TOP 1 1 FROM backupset WHERE database_name = 'C'
            var hasBackupFull = await sqlConnection.CreateFastQuery()
                .ThrowIfIsSystemDb()
                .WithQuery($"USE msdb; SELECT TOP 1 1 FROM backupset WHERE database_name = '{dbName}'; USE {dbName}; ")
                .ExecuteScalarAsync<int?>();
            if (hasBackupFull != 1)
            {
                Debug.WriteLine("Database must backup full before Backup log.");
                return false;
            }

            //backup log
            var fullPath = Path.GetFullPath(pathFile);
            var query = GetQueryBackup(dbName, fullPath);
            await sqlConnection.CreateFastQuery()
               .ThrowIfIsSystemDb()
               .WithQuery(query)
               .ExecuteNonQueryAsync();
            return true;
        }

        protected abstract string GetQueryBackup(string dbName, string pathFile);

        public virtual async Task<bool> RestoreBackupAsync(SqlConnection sqlConnection, string pathFile, string minVersion)
        {
            var dbName = sqlConnection.Database;
            var fullPath = Path.GetFullPath(pathFile);
            var query = GetQueryRestore(dbName, fullPath, minVersion);
            await sqlConnection.CreateFastQuery()
                .WithQuery(query)
                .ExecuteNonQueryAsync();
            return true;
        }

        public string GetFileStandBy(string dbName, string pathFile, string minVersion)
        {
            var dir = Path.GetDirectoryName(pathFile) ?? dbName;
            var path = Path.Combine(dir, $"{dbName}.{minVersion}.standby");
            return Path.GetFullPath(path);
        }

        protected abstract string GetQueryRestore(string dbName, string pathFile, string minVersion);
    }
}
