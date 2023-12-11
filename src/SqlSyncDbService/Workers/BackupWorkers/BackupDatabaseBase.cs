using FastQueryLib;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using Microsoft.Net.Http.Headers;
using SqlSyncDbService.Workers.Interfaces;
using System.Reflection;

namespace SqlSyncLib.Workers.BackupWorkers
{
    public abstract class BackupDatabaseBase : IFileRestore
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
            var query = GetQueryBackup(dbName, pathFile);
            var backupSuccess = await ApplyAsync(sqlConnectString, query);
            return backupSuccess;
        }

        protected abstract string GetQueryBackup(string dbName, string pathFile);

        public virtual async Task<bool> RestoreBackupAsync(string sqlConnectString, string pathFile)
        {
            var dbName = new SqlConnectionStringBuilder(sqlConnectString).InitialCatalog;
            var query = GetQueryRestore(dbName, pathFile);
            var backupSuccess = await ApplyAsync(sqlConnectString, query);
            return backupSuccess;
        }

        protected abstract string GetQueryRestore(string dbName, string pathFile);

        public async Task<IFileRestore> BackupAsync(IWorkerConfig workerConfig, string pathFile)
        {
            var sqlConnectString = workerConfig?.SqlConnectString ?? throw new ArgumentNullException(nameof(workerConfig.SqlConnectString));
            var tempFile = Path.GetTempFileName();
            if (await CreateBackupAsync(sqlConnectString, tempFile))
            {
                return this;
            }
            throw new Exception("Create backup fail!");
        }

        public Task<bool> RestoreAsync(IWorkerConfig workerConfig, string pathFile)
        {
            throw new NotImplementedException();
        }
    }
}
