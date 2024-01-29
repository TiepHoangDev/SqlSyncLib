using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SqlSyncDbServiceLib.Interfaces;
using Microsoft.Extensions.Logging;

namespace SqlSyncDbServiceLib.LoggerWorkers
{
    public class LoggerWorkerHook : IWorkerHook
    {
        readonly ISqlSyncDbServiceLibLogger _logger;

        public LoggerWorkerHook(ISqlSyncDbServiceLibLogger logger)
        {
            _logger = logger;
        }

        public string Name => "Debug.WriteLine";
        public Task PostData(string name, object data)
        {
            Debug.WriteLine($"{name}>> {data}");
            if (data is Exception ex)
            {
                _logger?.Log(ex);
            }
            else if (data is IWorkerState state)
            {
                if (state.IsSuccess == false)
                {
                    _logger?.Log(state.ToString());
                }
            }
            return Task.CompletedTask;
        }
    }
}
