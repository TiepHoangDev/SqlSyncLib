using System.Diagnostics;
using SqlSyncDbService.Workers.Interfaces;

namespace SqlSyncDbService.Workers.LoggerWorkers
{
    public class LoggerWorkerHook : IWorkerHook
    {
        public Task PostData(string? name, object data)
        {
            Debug.WriteLine($"{name}>> {data}");
            return Task.CompletedTask;
        }
    }
}
