using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.LoggerWorkers
{
    /// <summary>
    /// Only log when failed: Exception or not success.
    /// </summary>
    public class FailedLoggerWorkerHook : IWorkerHook
    {
        readonly ISqlSyncDbServiceLibLogger _logger;

        public FailedLoggerWorkerHook(ISqlSyncDbServiceLibLogger logger)
        {
            _logger = logger;
        }

        public string Name => nameof(FailedLoggerWorkerHook);
        public async Task PostData(string name, object data)
        {
            if (data is Exception ex)
            {
                _logger.Log(ex);
                return;
            }
            if (data is IWorkerState state)
            {
                if (state.IsSuccess == false)
                {
                    _logger.Log(state.ToString());
                    return;
                }
            }
            
            Debug.WriteLine($"{name}>> {data}");
            await Task.CompletedTask;
        }
    }
}
