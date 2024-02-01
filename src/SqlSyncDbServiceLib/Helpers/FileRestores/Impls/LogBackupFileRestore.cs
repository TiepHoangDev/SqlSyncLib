using SqlSyncDbServiceLib.Helpers.FileRestores;
using SqlSyncDbServiceLib.Helpers.ScriptsDb;

namespace SqlSyncDbServiceLib.Helpers.FileRestores.Impls
{
    public class LogBackupFileRestore : BackupFileRestore
    {
        public override string Name => "Restore-LOG-Backup";
        protected override BackupDatabaseBase BackupDatabase => new LogBackupDatabase();
    }
}
