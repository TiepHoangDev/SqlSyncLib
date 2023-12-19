using System.Diagnostics;
using SqlSyncDbService.Workers.Interfaces;

namespace SqlSyncDbService.Workers.LoggerWorkers
{
    public class LoggerWorkerHook : IWorkerHook
    {
        readonly ILogger? _logger;

        public LoggerWorkerHook(ILogger? logger)
        {
            _logger = logger;
        }

        public string Name => "Debug.WriteLine";
        public Task PostData(string? name, object data)
        {
            Debug.WriteLine($"{name}>> {data}");
            if (data is Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
            }
            else if (data is IWorkerState state)
            {
                if (state.IsSuccess == false)
                {
                    _logger?.LogError(state.ToString());
                }
            }
            return Task.CompletedTask;
        }
    }
}
