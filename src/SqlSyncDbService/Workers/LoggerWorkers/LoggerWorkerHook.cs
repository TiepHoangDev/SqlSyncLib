using System.Diagnostics;
using SqlSyncDbService.Workers.Interfaces;

namespace SqlSyncDbService.Workers.LoggerWorkers
{
    public record LoggerWorkerHook : IWorkerHook
    {
        public string Name => "Debug.WriteLine";
        public Task PostData(string? name, object data)
        {
            Debug.WriteLine($"{name}>> {data}");
            return Task.CompletedTask;
        }
    }
}
