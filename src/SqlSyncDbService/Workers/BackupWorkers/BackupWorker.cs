using Microsoft.AspNetCore.Mvc;
using SqlSyncDbService.Workers.Helpers;
using SqlSyncDbService.Workers.Interfaces;
using System.Diagnostics;

namespace SqlSyncLib.Workers.BackupWorkers
{
    public class BackupWorker : WorkerBase
    {
        public override string Name => $"BackupWorker-{BackupConfig.DbName}";
        public override IWorkerConfig Config => BackupConfig;
        public BackupWorkerConfig BackupConfig { get; set; } = new BackupWorkerConfig();
        public BackupWorkerState BackupState { get; } = new BackupWorkerState();

        public override IWorkerState State => BackupState;

        public override async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                //backup
                await State.UpdateStateByProcess(async () =>
                {
                    var isExistbackupFull = BackupConfig.IsExistBackupFull(BackupState.MinVersion);

                    if (!isExistbackupFull || BackupConfig.IsReset(DateTime.Now))
                    {
                        await BackupFullAsync();
                    }
                    else
                    {
                        await BackupLogAsync();
                    }
                });
                CallHookAsync("BackupSuccess", BackupState);

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

        public string GetFileBackup(string versionToDownload)
        {
            return BackupConfig.GetPathFile(BackupState.MinVersion, versionToDownload);
        }

        public string GetNextVersion(string? currentVersion)
        {
            return BackupConfig.GetNextVersion(currentVersion, BackupState);
        }
    }

}
