using SqlSyncDbService.Workers.BackupWorkers;

namespace SqlSyncDbService.Workers.Helpers
{
    public class LogBackupFileBackup : BackupFileBackup
    {
        public readonly BackupWorkerState workerState;
        public LogBackupFileBackup(BackupWorkerState workerState)
        {
            this.workerState = workerState;
        }

        public override HeaderFile Header => new(typeof(LogBackupFileRestore).FullName!, workerState);
        protected override BackupDatabaseBase BackupDatabase => new LogBackupDatabase();
    }
}
