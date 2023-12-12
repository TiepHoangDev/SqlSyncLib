namespace SqlSyncDbService.Workers.Interfaces
{
    public interface IWorkerConfig
    {
        string Id { get; }
        TimeSpan DelayTime { get; }
        string? SqlConnectString { get; }
    }
}
