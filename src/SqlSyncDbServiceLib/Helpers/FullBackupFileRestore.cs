namespace SqlSyncDbServiceLib.Helpers
{
    public class FullBackupFileRestore : BackupFileRestore
    {
        public override string Name => "Restore-FULL-Backup";

        protected override BackupDatabaseBase BackupDatabase => new FullBackupDatabase();
    }
}
