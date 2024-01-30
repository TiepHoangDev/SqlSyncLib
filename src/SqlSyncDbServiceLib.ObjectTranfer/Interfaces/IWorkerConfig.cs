using System;

namespace SqlSyncDbServiceLib.ObjectTranfer.Interfaces
{
    public interface IWorkerConfig
    {
        string Id { get; }
        TimeSpan DelayTime { get; }
        string SqlConnectString { get; }
        EnumWorkerMode workerMode { get; }
    }
}
