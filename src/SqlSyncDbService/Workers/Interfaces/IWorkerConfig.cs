using SqlSyncDbService.Workers.RestoreWorkers;
using SqlSyncLib.Workers.BackupWorkers;
using System.Xml.Linq;

namespace SqlSyncDbService.Workers.Interfaces
{
    public interface IWorkerConfig
    {
        string Id { get; }
        TimeSpan DelayTime { get; }
        string? SqlConnectString { get; }
    }
}
