using SqlSyncDbServiceLib.ObjectTranfer.Instances;

namespace SqlSyncDbServiceLib.Helpers
{
    public class FullBackupFileBackup : BackupFileBackup
    {
        public readonly BackupWorkerState WorkerState;

        public FullBackupFileBackup(BackupWorkerState workerState)
        {
            WorkerState = workerState;
        }

        public override HeaderFile Header => new HeaderFile(typeof(FullBackupFileRestore).FullName, WorkerState);
        public override BackupDatabaseBase BackupDatabase { get; protected set; } = new FullBackupDatabase();
    }
}
