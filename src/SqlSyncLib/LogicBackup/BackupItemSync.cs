using FastQueryLib;
using Microsoft.Data.SqlClient;
using SqlSyncLib.Interfaces;
using SqlSyncLib.LogicBase;

namespace SqlSyncLib.LogicBackup
{
    public abstract record BackupItemSync(SqlSyncMetadata Metadata) : IItemSync
    {
        public virtual EnumTypeSync TypeSync => EnumTypeSync.BackupFull;
        public virtual string? FileBackup { get; private set; }

        public void PrepareApply(string fileBackupFull)
        {
            FileBackup = fileBackupFull;
        }

        public abstract string QueryRestore(string dbName);

        public async Task<bool> ApplyAsync(InputApplyItemSync inputApply)
        {
            using var conn = new SqlConnection(inputApply.ConnectString);
            var dbName = conn.Database;
            using var master = conn.NewOpenConnectToDatabase("master");
            var query = QueryRestore(dbName);
            using var restoreJob = await master.CreateFastQuery().WithQuery(query).ExecuteNumberOfRowsAsync();
            return await master.CheckDatabaseExistsAsync(dbName);
        }

    }
}
