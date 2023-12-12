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
        public override IWorkerState State => RestoreState;

        public override async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await State.UpdateStateByProcess(async () =>
                {
                    //download file
                    await DownloadNewBackup();
                    //Restore
                    await Restore();
                });
                CallHookAsync("RestoreWorker", RestoreState);

                if (cancellationToken.IsCancellationRequested) break;
                await Task.Delay(RestoreConfig.DelayTime, cancellationToken);
            }
            return true;
        }

        private async Task Restore()
        {
            var file = RestoreConfig.GetFilePath(RestoreState.CurrentVersion);
            if (!File.Exists(file)) return;

            var restore = await FileRestoreFactory.GetFileRestoreAsync(file);
            await restore.RestoreAsync(RestoreConfig, file);
        }

        private async Task DownloadNewBackup()
        {
            if (string.IsNullOrWhiteSpace(RestoreConfig.BackupAddress))
            {
                throw new ArgumentNullException(nameof(RestoreConfig.BackupAddress));
            }

            if (string.IsNullOrWhiteSpace(RestoreConfig.IdBackupWorker))
            {
                throw new ArgumentNullException(nameof(RestoreConfig.IdBackupWorker));
            }

            var filePath = RestoreConfig.GetFilePath(RestoreState.CurrentVersion);
            if (File.Exists(filePath)) return;

            using var client = new HttpClient
            {
                BaseAddress = new Uri(RestoreConfig.BackupAddress),
            };

            var request = new GetNewBackupRequest(RestoreConfig.IdBackupWorker)
            {
                currentVersion = RestoreState.CurrentVersion
            };
            using var response = await client.PostAsJsonAsync(GetNewBackupRequest.router, request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"{response.StatusCode}/{response.RequestMessage?.Method}: {response.RequestMessage?.RequestUri}. {body}");
            }

            var filename = response.Content.Headers.ContentDisposition?.FileName;
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new Exception($"{response.StatusCode}/{response.RequestMessage?.Method}: {response.RequestMessage?.RequestUri}. Unknow filename of response");
            }
            if (RestoreState.CurrentVersion == null)
            {
                RestoreState.CurrentVersion = filename;
                filePath = RestoreConfig.GetFilePath(RestoreState.CurrentVersion);
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var fs = new FileStream(filePath, FileMode.Create);
            stream.CopyTo(fs);
            fs.Flush();


        }
    }
}
