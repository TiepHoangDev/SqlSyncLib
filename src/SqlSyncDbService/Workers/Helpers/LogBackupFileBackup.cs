namespace SqlSyncDbService.Workers.Helpers
{
    public class LogBackupFileBackup : BackupFileBackup
    {
        public override HeaderFile Header => new HeaderFile(typeof(LogBackupFileRestore).FullName!);
        protected override BackupDatabaseBase BackupDatabase => new LogBackupDatabase();
    }
}
