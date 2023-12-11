namespace SqlSyncDbService.Workers.Helpers
{
    public class FullBackupFileRestore : BackupFileRestore
    {
        protected override BackupDatabaseBase BackupDatabase => new FullBackupDatabase();
    }
}
