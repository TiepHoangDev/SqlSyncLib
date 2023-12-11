namespace SqlSyncDbService.Workers.Interfaces
{
    public interface IWorkerConfig
    {
        TimeSpan DelayTime { get; }
        DateTime? LastRun { get; }
        string? SqlConnectString { get; }
    }
}
