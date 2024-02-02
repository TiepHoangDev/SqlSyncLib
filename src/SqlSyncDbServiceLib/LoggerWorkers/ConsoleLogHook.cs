using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.LoggerWorkers
{
    public class ConsoleLogHook : IWorkerHook
    {
        public string Name => nameof(ConsoleLogHook);

        public async Task PostData(string name, object data)
        {
            Debug.WriteLine($"{name} {data}");
            Console.WriteLine($"{name} {data}");
            await Task.CompletedTask;
        }
    }
}
