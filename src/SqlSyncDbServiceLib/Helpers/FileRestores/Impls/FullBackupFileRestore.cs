using SqlSyncDbServiceLib.Helpers.FileRestores;
using SqlSyncDbServiceLib.Helpers.ScriptsDb;

namespace SqlSyncDbServiceLib.Helpers.FileRestores.Impls
{
    public class FullBackupFileRestore : BackupFileRestore
    {
        public override string Name => "Restore-FULL-Backup";

        protected override BackupDatabaseBase BackupDatabase => new FullBackupDatabase();
    }
}
