namespace SqlSyncDbService.Models
{
    public class WorkerLogger : SqlSyncDbServiceLib.Interfaces.ILogger
    {
        private readonly ILogger _logger;

        public WorkerLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void LogError(Exception ex, string message) => _logger.LogError(ex, message);

        public void LogError(string message) => _logger.LogError(message);
    }
}
