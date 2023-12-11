namespace SqlSyncDbService.Workers.Helpers
{
    public class FullBackupFileBackup : BackupFileBackup
    {
        public override HeaderFile Header => new HeaderFile(typeof(FullBackupFileRestore).FullName!);
        protected override BackupDatabaseBase BackupDatabase => new FullBackupDatabase();
    }
}
