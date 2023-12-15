using FastQueryLib;
using Microsoft.Data.SqlClient;

namespace SqlSyncDbService.Workers.Helpers
{
    public class FullBackupDatabase : BackupDatabaseBase
    {
        protected override string GetQueryBackup(string dbName, string pathFile)
        {
            var query = $"USE master; ALTER DATABASE [{dbName}] SET RECOVERY FULL;  BACKUP DATABASE [{dbName}] TO DISK='{pathFile}' WITH FORMAT; USE [{dbName}];";
            return query;
        }

        public override async Task<bool> RestoreBackupAsync(string sqlConnectString, string pathFile, string minVersion)
        {
            var builder = new SqlConnectionStringBuilder(sqlConnectString);
            var dbName = builder.InitialCatalog;

            //build query
            var fullPath = Path.GetFullPath(pathFile);
            var queryRestore = GetQueryRestore(dbName, fullPath, minVersion);
            var query = $"USE master; RESTORE FILELISTONLY FROM DISK = '{fullPath}';";
            using (var con = new SqlConnection(sqlConnectString))
            {
                using var result = await con.CreateFastQuery()
                    .UseDatabase("master")
                    .WithQuery(query)
                    .ExecuteReadAsyncAs<RESTORE_FILELISTONLY_Record>();
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
            }

            //set to single-user
            if (await new SqlConnection(sqlConnectString).CreateFastQuery().CheckDatabaseExistsAsync())
            {
                var queryDropDb = $"USE master; DROP DATABASE [{dbName}];";
                using var _ = await new SqlConnection(sqlConnectString)
                    .CreateFastQuery()
                    .UseDatabase("master")
                    .WithQuery(queryDropDb)
                    .ExecuteNonQueryAsync();
            }

            //restore
            using var restore_dbcopy = await new SqlConnection(sqlConnectString).CreateFastQuery()
                .UseDatabase("master")
                .WithQuery(queryRestore)
                .ExecuteNonQueryAsync();
            return await new SqlConnection(sqlConnectString).CreateFastQuery().CheckDatabaseExistsAsync();
        }

        protected override string GetQueryRestore(string dbName, string pathFile, string minVersion)
        {
            var dir = Path.GetDirectoryName(pathFile) ?? dbName;
            var standby = Path.Combine(dir, $"{dbName}-{minVersion}.standby");
            var query = $"USE master; RESTORE DATABASE [{dbName}] FROM DISK='{pathFile}' WITH REPLACE, STANDBY='{standby}' ";
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
