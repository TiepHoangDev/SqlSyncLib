using SqlSyncDbServiceLib.ObjectTranfer.Instances;
using System.Collections.Generic;

namespace SqlSyncDbServiceLib.ObjectTranfer.Interfaces
{
    public interface IManageWorkerLogic
    {
        List<IWorker> AddBackupWorker(BackupWorkerConfig config);
        List<IWorker> AddRestoreWorker(RestoreWorkerConfig config);
        GetNewBackupResponse GetNewBackup(GetNewBackupRequest getFileBackup);
        List<IWorker> GetWorkers(List<string> ids);
        bool RemoveWorker(string id);
    }
}