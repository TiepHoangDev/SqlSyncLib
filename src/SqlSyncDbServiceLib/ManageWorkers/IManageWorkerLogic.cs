using SqlSyncDbServiceLib.BackupWorkers;
using SqlSyncDbServiceLib.Interfaces;
using SqlSyncDbServiceLib.RestoreWorkers;
using System.Collections.Generic;

namespace SqlSyncDbServiceLib.ManageWorkers
{
    public interface IManageWorkerLogic
    {
        List<IWorker> AddBackupWorker(BackupWorkerConfig config);
        List<IWorker> AddRestoreWorker(RestoreWorkerConfig config);
        GetNewBackupResponse GetNewBackup(GetNewBackupRequest getFileBackup);
        List<IWorker> GetWorkers(List<string> ids = null);
        bool RemoveWorker(string id);
    }
}