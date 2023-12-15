using SqlSyncLib.Workers.BackupWorkers;

namespace SqlSyncDbService.Workers.Helpers
{
    public class FullBackupFileBackup : BackupFileBackup
    {
        public readonly BackupWorkerState WorkerState;

        public FullBackupFileBackup(BackupWorkerState workerState)
        {
            WorkerState = workerState;
        }

        public override HeaderFile Header => new(typeof(FullBackupFileRestore).FullName!, WorkerState);
        protected override BackupDatabaseBase BackupDatabase => new FullBackupDatabase();
    }
}
