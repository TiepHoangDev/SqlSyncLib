namespace SqlSyncDbService.Workers.Helpers
{
    public class LogBackupFileRestore : BackupFileRestore
    {
        protected override BackupDatabaseBase BackupDatabase => new LogBackupDatabase();
    }
}
