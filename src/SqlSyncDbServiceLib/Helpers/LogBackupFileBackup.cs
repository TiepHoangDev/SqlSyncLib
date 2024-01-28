using SqlSyncDbServiceLib.BackupWorkers;

namespace SqlSyncDbServiceLib.Helpers
{
    public class LogBackupFileBackup : BackupFileBackup
    {
        public readonly BackupWorkerState workerState;
        public LogBackupFileBackup(BackupWorkerState workerState)
        {
            this.workerState = workerState;
        }

        public override HeaderFile Header => new HeaderFile(typeof(LogBackupFileRestore).FullName, workerState);

        public override BackupDatabaseBase BackupDatabase { get; protected set; } = new LogBackupDatabase();
    }
}
