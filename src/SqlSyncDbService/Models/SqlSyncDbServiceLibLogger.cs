using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;

namespace SqlSyncDbService.Models
{
    public class SqlSyncDbServiceLibLogger : IDbServiceLibLogger
    {
        private readonly ILogger<SqlSyncDbServiceLibLogger> _logger;

        public SqlSyncDbServiceLibLogger(ILogger<SqlSyncDbServiceLibLogger> logger)
        {
            _logger = logger;
        }
        public void Log(object message) => _logger.LogInformation(message?.ToString());
    }
}
