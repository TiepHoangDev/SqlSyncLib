using System.Diagnostics;
using SqlSyncDbService.Workers.Interfaces;

namespace SqlSyncLib.Workers.BackupWorkers
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
