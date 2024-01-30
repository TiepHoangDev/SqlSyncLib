using SqlSyncDbServiceLib.ObjectTranfer.Instances;
using System.Collections.Generic;

namespace SqlSyncDbServiceLib.ObjectTranfer.Interfaces
{
    public interface ILoaderConfig
    {
        List<BackupWorkerConfig> BackupWorkerConfigs { get; }
        List<RestoreWorkerConfig> RestoreWorkerConfigs { get; }
    }
}
