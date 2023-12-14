using FastQueryLib;
using Microsoft.Data.SqlClient;

namespace SqlSyncDbService.Workers.Helpers
{
    public class FullBackupDatabase : BackupDatabaseBase
    {
        protected override string GetQueryBackup(string dbName, string pathFile)
        {
            var query = $" ALTER DATABASE [{dbName}] SET RECOVERY FULL;  BACKUP DATABASE [{dbName}] TO DISK='{pathFile}' WITH FORMAT; ";
            return query;
        }

        public override async Task<bool> RestoreBackupAsync(string sqlConnectString, string pathFile)
        {
            using var conn = new SqlConnection(sqlConnectString);
            using var master_connection = conn.NewOpenConnectToDatabase("master");
            var dbName = conn.Database;
            var fullPath = Path.GetFullPath(pathFile);

            var minVersion = dbName; // VersionFactory.Instance.GetNewVersion();
            var queryRestore = GetQueryRestore(dbName, fullPath, minVersion);
            var query = $"RESTORE FILELISTONLY FROM DISK = '{fullPath}';";
            using var result = await master_connection.CreateFastQuery().WithQuery(query).ExecuteReadAsyncAs<RESTORE_FILELISTONLY_Record>();
            if (result.Result.Any())
            {
                string getMoveQuery(RESTORE_FILELISTONLY_Record filelistonly_record, string id)
                {
                    var extention = Path.GetExtension(filelistonly_record.PhysicalName) ?? throw new NullReferenceException(nameof(filelistonly_record.PhysicalName));
                    var dir = Path.GetDirectoryName(filelistonly_record.PhysicalName) ?? throw new NullReferenceException(nameof(filelistonly_record.PhysicalName));
                    var newPath = Path.Combine(dir, $"{dbName}_{id}{extention}");
                    return $"MOVE N'{filelistonly_record.LogicalName}' TO N'{newPath}'";
                }

                var id = VersionFactory.Instance.GetNewVersion();
                var queryMoves = result.Result.Select(q => getMoveQuery(q, id)).ToList();
                queryMoves.Insert(0, queryRestore);
                queryRestore = string.Join(", ", queryMoves);
            }

            using var restore_dbcopy = await master_connection.CreateFastQuery()
                .WithQuery(queryRestore)
                .ExecuteNonQueryAsync();
            return true;
        }

        protected override string GetQueryRestore(string dbName, string pathFile, string version)
        {
            var dir = Path.GetDirectoryName(pathFile) ?? dbName;
            var standby = Path.Combine(dir, $"{dbName}-{version}.standby");
            var query = $" RESTORE DATABASE [{dbName}] FROM DISK='{pathFile}' WITH REPLACE, STANDBY='{standby}' ";
            return query;
        }

        record RESTORE_FILELISTONLY_Record
        {
            public string? LogicalName { get; set; }
            public string? PhysicalName { get; set; }
            public string? Type { get; set; }
            public string? FileGroupName { get; set; }
        }
    }
}
