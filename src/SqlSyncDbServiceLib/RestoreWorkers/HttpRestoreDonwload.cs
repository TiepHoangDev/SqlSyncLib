﻿using Newtonsoft.Json;
using SqlSyncDbServiceLib.ObjectTranfer;
using SqlSyncDbServiceLib.ObjectTranfer.Instances;
using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
                var request = new GetNewBackupRequest()
                {
                    CurrentVersion = RestoreState.DownloadedVersion,
                    IdBackupWorker = RestoreConfig.IdBackupWorker,
                };
                var url = RestoreConfig.GetUrlDownload();
                using (var response = await client.PostAsJsonAsync(url, request, cancellationToken))
                {
                    //check response
                    if (!response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        throw new Exception($"{response.StatusCode}/{response.RequestMessage?.Method}: {response.RequestMessage?.RequestUri} body={body}");
                    }

                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        Debug.WriteLine("not have new file backup");
                        return default;
                    }

                    //get version
                    var version = response.Content.Headers.ContentDisposition?.FileName;
                    if (string.IsNullOrWhiteSpace(version))
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        throw new Exception($"{response.StatusCode}/{response.RequestMessage?.Method}: {response.RequestMessage?.RequestUri}. Unknow filename(is version) of response, that is required of response. body={body}");
                    }

                    //save file
                    var file = RestoreConfig.GetFilePathData(version);
                    using (var fs = new FileStream(file, FileMode.Create))
                    {
                        await response.Content.CopyToAsync(fs);
                        await fs.FlushAsync(cancellationToken);

                        return version;
                    }
                }
            }
        }
    }
}
