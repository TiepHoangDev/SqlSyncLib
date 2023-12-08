using Microsoft.AspNetCore.Mvc;
using SqlSyncDbService.Workers.Interfaces;
using System.Diagnostics;

namespace SqlSyncLib.Workers.BackupWorkers
{
    public class BackupWorker : WorkerBase
    {
        public override string Name => $"BackupWorker-{BackupConfig.DbName}";
        public override IWorkerConfig Config => BackupConfig;
        public BackupWorkerConfig BackupConfig { get; } = new BackupWorkerConfig();
        public BackupWorkerState BackupState { get; } = new BackupWorkerState();

        public override async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                //backup
                try
                {
                    if (BackupConfig.IsReset(DateTime.Now))
                    {
                        await BackupFullAsync();
                    }
                    else
                    {
                        await BackupLogAsync();
                    }
                    CallHookAsync("BackupSuccess", BackupState);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    CallHookAsync("BackupError", ex);
                }

                if (cancellationToken.IsCancellationRequested) break;
                await Task.Delay(BackupConfig.DelayTime, cancellationToken);
            }
            return true;
        }

        public async Task<bool> BackupLogAsync()
        {
            return await new BackupWorkerApi().BackupLog(BackupConfig, BackupState);
        }

        public async Task<bool> BackupFullAsync()
        {
            return await new BackupWorkerApi().BackupFull(BackupConfig, BackupState);
        }

        public string GetFileBackup(string version)
        {
            if (version.CompareTo(BackupState.CurrentVersion) > 0) 
                throw new Exception($"version={version} is not valid. CurrentVersion is {BackupState.CurrentVersion}");

            var downloadVersion = version;
            if (version.CompareTo(BackupState.MinVersion) < 0)
            {
                downloadVersion = BackupState.MinVersion;
            }

            return BackupConfig.GetPathFile(BackupState.MinVersion, downloadVersion);
        }
    }

}
