using SqlSyncDbService.Workers.Interfaces;

namespace SqlSyncDbService.Services
{
    public class ManageWorkerService : BackgroundService
    {
        private IManageWorker _manageWorker;

        public ManageWorkerService(IManageWorker manageWorker)
        {
            _manageWorker = manageWorker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _manageWorker.RunAsync(stoppingToken);
        }
    }
}
