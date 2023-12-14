using SqlSyncDbService.Workers.ManageWorkers;
using System.Diagnostics;

namespace SqlSyncDbService.Workers.RestoreWorkers
{
    public class HttpRestoreDonwload : IRestoreDownload
    {
        public async Task<string?> DownloadFileAsync(RestoreWorkerConfig RestoreConfig, RestoreWorkerState RestoreState, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(RestoreConfig.BackupAddress))
            {
                throw new NullReferenceException($"Please provider url server backup to conenect: {nameof(RestoreConfig.BackupAddress)}");
            }

            if (string.IsNullOrWhiteSpace(RestoreConfig.IdBackupWorker))
            {
                throw new NullReferenceException($"Please provider id-backup-worker on server backup: {nameof(RestoreConfig.IdBackupWorker)}");
            }
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
                    Debug.WriteLine("not have new file backup");
                    return default;
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

            //save file
            var file = RestoreConfig.GetFilePathData(version);
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fs = new FileStream(file, FileMode.Create);
            await stream.CopyToAsync(fs, cancellationToken);
            await fs.FlushAsync(cancellationToken);

            return version;
        }
    }
}
