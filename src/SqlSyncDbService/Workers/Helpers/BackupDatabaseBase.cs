using FastQueryLib;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using Microsoft.Net.Http.Headers;
using SqlSyncDbService.Workers.Interfaces;
using System.Reflection;

namespace SqlSyncDbService.Workers.Helpers
{
    public abstract class BackupDatabaseBase
    {
        public virtual async Task<bool> ApplyAsync(string sqlConnectString, string query)
        {
            using var conn = new SqlConnection(sqlConnectString);
            var dbName = conn.Database;
            using var master = conn.NewOpenConnectToDatabase("master");
            using var restoreJob = await master.CreateFastQuery().WithQuery(query).ExecuteNumberOfRowsAsync();
            return await master.CheckDatabaseExistsAsync(dbName);
        }

        public virtual async Task<bool> CreateBackupAsync(string sqlConnectString, string pathFile)
        {
            var dbName = new SqlConnectionStringBuilder(sqlConnectString).InitialCatalog;
            var fullPath = Path.GetFullPath(pathFile);
            var query = GetQueryBackup(dbName, fullPath);
            var backupSuccess = await ApplyAsync(sqlConnectString, query);
            return backupSuccess;
        }

        protected abstract string GetQueryBackup(string dbName, string pathFile);

        public virtual async Task<bool> RestoreBackupAsync(string sqlConnectString, string pathFile)
        {
            var dbName = new SqlConnectionStringBuilder(sqlConnectString).InitialCatalog;
            var fullPath = Path.GetFullPath(pathFile);
            var query = GetQueryRestore(dbName, fullPath);
            var backupSuccess = await ApplyAsync(sqlConnectString, query);
            return backupSuccess;
        }

        protected abstract string GetQueryRestore(string dbName, string pathFile);
    }
}
