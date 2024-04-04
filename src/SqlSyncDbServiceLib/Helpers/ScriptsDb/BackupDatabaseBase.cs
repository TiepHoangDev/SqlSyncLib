using FastQueryLib;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.Helpers.ScriptsDb
{
    public abstract class BackupDatabaseBase
    {
        public virtual async Task<bool> CreateBackupAsync(SqlConnection sqlConnection, string pathFile)
        {
            var dbName = sqlConnection.Database;
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

        public async Task<bool> CheckHasBackupFullAsync(FastQuery fastQuery)
        {
            var dbName = fastQuery.Database;

            //check database has backup full before
            var hasBackupFull = await fastQuery
                .ThrowIfIsSystemDb()
                .WithQuery($"USE msdb; SELECT TOP 1 1 FROM backupset WHERE database_name = '{dbName}'; USE {dbName}; ")
                .ExecuteScalarAsync<int?>();
            return hasBackupFull == 1;
        }
    }
}
