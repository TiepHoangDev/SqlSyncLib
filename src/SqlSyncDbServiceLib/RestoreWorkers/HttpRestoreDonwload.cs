using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using SqlSyncDbServiceLib.ManageWorkers;

namespace SqlSyncDbServiceLib.RestoreWorkers
{
    public class HttpRestoreDonwload : IRestoreDownload
    {
        public async Task<string> DownloadFileAsync(RestoreWorkerConfig RestoreConfig, RestoreWorkerState RestoreState, CancellationToken cancellationToken)
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
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(RestoreConfig.BackupAddress);

                var request = new GetNewBackupRequest()
                {
                    CurrentVersion = RestoreState.DownloadedVersion,
                    DbId = RestoreConfig.IdBackupWorker,
                };
                using (var response = await client.PostAsJsonAsync(GetNewBackupRequest.router, request, cancellationToken))
                {
                    //check response
                    var body = await response.Content.ReadAsStringAsync();
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NoContent:
                            Debug.WriteLine("not have new file backup");
                            return default;
                        case HttpStatusCode.OK:
                            break;
                        default:
                            throw new Exception($"{response.StatusCode}/{response.RequestMessage?.Method}: {response.RequestMessage?.RequestUri}. {body}");
                    }

                    //get version
                    var backupResponse = JsonConvert.DeserializeObject<GetNewBackupResponse>(body);
                    var version = backupResponse.Version ?? response.Content.Headers.ContentDisposition?.FileName;
                    if (string.IsNullOrWhiteSpace(version))
                    {
                        throw new Exception($"{response.StatusCode}/{response.RequestMessage?.Method}: {response.RequestMessage?.RequestUri}. Unknow filename(=version) of response, that is required of response.");
                    }

                    //save file
                    var file = RestoreConfig.GetFilePathData(version);
                    using (var fs = new FileStream(file, FileMode.Create))
                    {
                        await backupResponse.FileStream.CopyToAsync(fs);
                        await fs.FlushAsync(cancellationToken);

                        return version;
                    }
                }
            }
        }
    }
}
