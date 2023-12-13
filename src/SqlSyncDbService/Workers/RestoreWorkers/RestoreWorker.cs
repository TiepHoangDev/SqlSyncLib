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

        protected override void _debug(string msg) => Debug.WriteLine($"\tRESTORE: {msg}");

        public override async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await State.UpdateStateByProcess(() => DownloadNewBackup(cancellationToken));
                await State.UpdateStateByProcess(() => Restore(cancellationToken));
                CallHookAsync("RestoreWorker", RestoreState);

                if (cancellationToken.IsCancellationRequested) break;
                await Task.Delay(RestoreConfig.DelayTime, cancellationToken);
            }
            return true;
        }

        private async Task Restore(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                //get file
                if (RestoreState.CurrentVersion == null) return;
                var state = RestoreConfig.GetStateByVersion(RestoreState.CurrentVersion);
                if (state?.CurrentVersion == null) return;
                var file = RestoreConfig.GetFilePathData(state.CurrentVersion);
                if (!File.Exists(file)) return;

                //restore
                var restore = await FileRestoreFactory.GetFileRestoreAsync(file);
                var ok = await restore.RestoreAsync(RestoreConfig, file);
                if (!ok)
                {
                    throw new Exception($"Restore file failed on {RestoreState}");
                }
                _debug($"success {RestoreState.CurrentVersion}");

                //update state
                RestoreState.CurrentVersion = state.NextVersion;
            }
        }

        /// <summary>
        /// send current version
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        private async Task DownloadNewBackup(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(RestoreConfig.BackupAddress))
            {
                throw new NullReferenceException($"Please provider url server backup to conenect: {nameof(RestoreConfig.BackupAddress)}");
            }

            if (string.IsNullOrWhiteSpace(RestoreConfig.IdBackupWorker))
            {
                throw new NullReferenceException($"Please provider id-backup-worker on server backup: {nameof(RestoreConfig.IdBackupWorker)}");
            }

            //check init value: DownloadedVersion
            RestoreState.DownloadedVersion = RestoreState.DownloadedVersion ?? RestoreState.CurrentVersion;

            var counter = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                //setup client
                using var client = new HttpClient
                {
                    BaseAddress = new Uri(RestoreConfig.BackupAddress),
                };
                var request = new GetNewBackupRequest(RestoreConfig.IdBackupWorker)
                {
                    CurrentVersion = RestoreState.DownloadedVersion
                };
                using var response = await client.PostAsJsonAsync(GetNewBackupRequest.router, request, cancellationToken);

                //check response
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.NoContent:
                        _debug("not have new file backup");
                        return;
                    case System.Net.HttpStatusCode.OK:
                        break;
                    default:
                        throw new Exception($"{response.StatusCode}/{response.RequestMessage?.Method}: {response.RequestMessage?.RequestUri}. {body}");
                }

                //get version
                var version = response.Content.Headers.ContentDisposition?.FileName;
                if (string.IsNullOrWhiteSpace(version))
                {
                    throw new Exception($"{response.StatusCode}/{response.RequestMessage?.Method}: {response.RequestMessage?.RequestUri}. Unknow filename(=version) of response, that is required of response.");
                }

                //save version for restore
                RestoreConfig.SaveState(new WorkerStateVersionBase
                {
                    CurrentVersion = RestoreState.DownloadedVersion,
                    NextVersion = version,
                });
                _debug($"{RestoreState.DownloadedVersion} => {version}");

                //update state
                RestoreState.DownloadedVersion = version;
                RestoreState.CurrentVersion = RestoreState.CurrentVersion ?? version;

                //save file
                var file = RestoreConfig.GetFilePathData(RestoreState.DownloadedVersion);
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var fs = new FileStream(file, FileMode.Create);
                await stream.CopyToAsync(fs, cancellationToken);
                await fs.FlushAsync(cancellationToken);

                //check limit
                if (RestoreConfig.MaxFileDownload > 0)
                {
                    counter++;
                    if (counter >= RestoreConfig.MaxFileDownload)
                    {
                        break;
                    }
                }

                //sleep
                await Task.Delay(500, cancellationToken);
            }
        }

    }
}
