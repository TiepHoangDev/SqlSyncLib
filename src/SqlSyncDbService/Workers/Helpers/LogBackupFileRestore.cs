namespace SqlSyncDbService.Workers.Helpers
{
    public class LogBackupFileRestore : BackupFileRestore
    {
        public override string Name => "Restore-LOG-Backup";
        protected override BackupDatabaseBase BackupDatabase => new LogBackupDatabase();
    }
}
