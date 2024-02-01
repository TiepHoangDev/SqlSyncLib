using SqlSyncDbServiceLib.Helpers.FileRestores.Impls;
using SqlSyncDbServiceLib.Helpers.ScriptsDb;
using SqlSyncDbServiceLib.ObjectTranfer.Instances;

namespace SqlSyncDbServiceLib.Helpers.FileBackups.Impls
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
