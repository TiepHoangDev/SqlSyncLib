using SqlSyncDbService.Workers.Helpers;
using SqlSyncDbService.Workers.Interfaces;
using SqlSyncDbService.Workers.ManageWorkers;
using System.Diagnostics;

namespace SqlSyncDbService.Workers.RestoreWorkers
{
    public class RestoreWorker : WorkerBase
    {
        public override string Name => "RestoreWorker";
        public override IWorkerConfig Config => RestoreConfig;
        public RestoreWorkerConfig RestoreConfig { get; set; } = new RestoreWorkerConfig();
        public RestoreWorkerState RestoreState { get; set; } = new RestoreWorkerState();

        public override async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    RestoreConfig.LastRun = DateTime.Now;

                    //download file
                    await DownloadNewBackup();
                    CallHookAsync("RestoreWorker_Download_File", RestoreConfig);

                    //Restore
                    await Restore();
                    CallHookAsync("RestoreWorker_Restore", RestoreConfig);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    CallHookAsync("RestoreWorker_Exception", ex);
                }

                if (cancellationToken.IsCancellationRequested) break;
                await Task.Delay(RestoreConfig.DelayTime, cancellationToken);
            }
            return true;
        }

        private async Task Restore()
        {
            var file = RestoreConfig.GetFilePath(RestoreState.CurrentVersion);
            var restore = await FileRestoreFactory.GetFileRestoreAsync(file);
            await restore.RestoreAsync(RestoreConfig, file);
        }

        private async Task DownloadNewBackup()
        {
            if (string.IsNullOrWhiteSpace(RestoreConfig.BackupAddress))
            {
                throw new ArgumentNullException(nameof(RestoreConfig.BackupAddress));
            }

            if (string.IsNullOrWhiteSpace(RestoreConfig.dbId))
            {
                throw new ArgumentNullException(nameof(RestoreConfig.dbId));
            }

            var filePath = RestoreConfig.GetFilePath(RestoreState.CurrentVersion);
            if (File.Exists(filePath)) return;

            using var client = new HttpClient
            {
                BaseAddress = new Uri(RestoreConfig.BackupAddress),
            };

            var request = new GetNewBackupRequest(RestoreConfig.dbId)
            {
                version = RestoreState.CurrentVersion
            };
            using var response = await client.PostAsJsonAsync(GetNewBackupRequest.router, request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"{response.RequestMessage?.RequestUri} => {response.StatusCode}. {body}");
            }

            if (response.Headers.TryGetValues("content-disposition", out var values))
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                using var fs = new FileStream(filePath, FileMode.Create);
                stream.CopyTo(fs);
                fs.Flush();

                if (RestoreState.CurrentVersion == null)
                {
                    RestoreState.CurrentVersion = values.FirstOrDefault();
                }
            }
        }
    }
}
