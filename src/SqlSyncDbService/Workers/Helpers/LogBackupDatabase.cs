using FastQueryLib;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Data.SqlClient;

namespace SqlSyncDbService.Workers.Helpers
{
    public class LogBackupDatabase : BackupDatabaseBase
    {
        protected override string GetQueryBackup(string dbName, string pathFile)
        {
            var query = $"USE master; BACKUP LOG [{dbName}] TO DISK='{pathFile}' WITH FORMAT; ";
            return query;
        }

        protected override string GetQueryRestore(string dbName, string pathFile, string minVersion)
        {
            var standby = GetFileStandBy(dbName, pathFile, minVersion);
            var query = $"USE master; RESTORE LOG [{dbName}] FROM DISK='{pathFile}' WITH STANDBY='{standby}'; USE [{dbName}]; ";
            return query;
        }

        public override async Task<bool> RestoreBackupAsync(string sqlConnectString, string pathFile, string minVersion)
        {
            var builder = new SqlConnectionStringBuilder(sqlConnectString);
            var dbName = builder.InitialCatalog;
            var setToMultiUser = false;
            try
            {
                using (var faster = builder.CreateOpenConnection().CreateFastQuery())
                {
                    if (!await faster.IsDatabaseSingleUserAsync())
                    {
                        //set signle user
                        setToMultiUser = true;
                        await faster.Clear().SetDatabaseSingleUserAsync(true);
                    }
                }
                //restore log
                var fullPath = Path.GetFullPath(pathFile);
                var queryRestore = GetQueryRestore(dbName, fullPath, minVersion);
                using var restore = await builder.CreateOpenConnection().CreateFastQuery()
                    .WithQuery(queryRestore)
                    .ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (setToMultiUser)
                {
                    //restore state
                    using var _ = await builder.CreateOpenConnection()
                        .CreateFastQuery()
                        .SetDatabaseSingleUserAsync(false);
                }
            }
        }
    }
}
