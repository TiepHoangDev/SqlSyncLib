using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using System.Collections.Generic;

namespace SqlSyncDbServiceLib.ObjectTranfer.Instances
{
    public class LoaderConfig : ILoaderConfig
    {
        public List<BackupWorkerConfig> BackupWorkerConfigs { get; set; } = new List<BackupWorkerConfig>();

        public List<RestoreWorkerConfig> RestoreWorkerConfigs { get; set; } = new List<RestoreWorkerConfig>();
    }
}
