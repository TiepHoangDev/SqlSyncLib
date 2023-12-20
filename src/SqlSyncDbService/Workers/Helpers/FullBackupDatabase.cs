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

        public override async Task<bool> RestoreBackupAsync(SqlConnection sqlConnection, string pathFile, string minVersion)
        {
            var dbName = sqlConnection.Database;

            //build query
            var fullPath = Path.GetFullPath(pathFile);
            var queryRestore = GetQueryRestore(dbName, fullPath, minVersion);
            var query = $"USE master; RESTORE FILELISTONLY FROM DISK = '{fullPath}';";

            using var result = await sqlConnection.CreateFastQuery()
                .UseDatabase("master")
                .WithQuery(query)
                .ExecuteReadAsyncAs<RESTORE_FILELISTONLY_Record>();
            if (result.Result.Any())
            {
                string getMoveQuery(RESTORE_FILELISTONLY_Record filelistonly_record, string id, string dir)
                {
                    var extention = Path.GetExtension(filelistonly_record.PhysicalName) ?? throw new NullReferenceException(nameof(filelistonly_record.PhysicalName));
                    var newPath = Path.Combine(dir, $"{dbName}_{id}{extention}");
                    return $"MOVE N'{filelistonly_record.LogicalName}' TO N'{newPath}'";
                }

                //create dir save primary file and log
                var dir = Path.GetDirectoryName(fullPath) ?? fullPath;
                dir = Path.GetDirectoryName(dir) ?? fullPath;
                dir = Path.Combine(dir, "database_files");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                //create query
                var id = VersionFactory.Instance.GetNewVersion();
                var queryMoves = result.Result.Select(q => getMoveQuery(q, id, dir)).ToList();
                queryMoves.Insert(0, queryRestore);
                queryRestore = string.Join(", ", queryMoves);
            }

            //set to single-user
            if (await sqlConnection.CreateFastQuery().CheckDatabaseExistsAsync())
            {
                var queryDropDb = $"USE master; DROP DATABASE [{dbName}];";
                using var _ = await sqlConnection
                    .CreateFastQuery()
                    .UseDatabase("master")
                    .WithQuery(queryDropDb)
                    .ExecuteNonQueryAsync();
            }

            //restore
            using var restore_dbcopy = await sqlConnection.CreateFastQuery()
                .UseDatabase("master")
                .WithQuery(queryRestore)
                .ExecuteNonQueryAsync();
            return await sqlConnection.CreateFastQuery().CheckDatabaseExistsAsync();
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
