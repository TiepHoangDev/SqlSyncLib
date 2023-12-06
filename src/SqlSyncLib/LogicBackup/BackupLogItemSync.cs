using SqlSyncLib.LogicBase;

namespace SqlSyncLib.LogicBackup
{
    public record BackupLogItemSync(SqlSyncMetadata Metadata) : BackupItemSync(Metadata)
    {
        public override string QueryRestore(string dbName)
        {
            return $"RESTORE LOG [{dbName}] FROM DISK='{FileBackup}'; ";
        }
    }
}
