using System.Xml.Linq;
using SqlSyncLib.LogicBase;

namespace SqlSyncLib.LogicBackup
{
    public record BackupFullItemSync(SqlSyncMetadata Metadata) : BackupItemSync(Metadata)
    {
        public override string QueryRestore(string dbName)
        {
            var standby = $"{dbName}.{Metadata.MinVersion}.standby";
            return $"RESTORE DATABASE [{dbName}] FROM DISK='{FileBackup}' WITH REPLACE, STANDBY='{standby}'; ";
        }
    }
}
