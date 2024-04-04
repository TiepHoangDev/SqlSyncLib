﻿using System.Data.SqlClient;
using SqlSyncDbServiceLib.Helpers;
using SqlSyncDbServiceLib.ObjectTranfer.Instances;
using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SqlSyncDbServiceLib.Helpers.FileBackups;

namespace SqlSyncDbServiceLib.RestoreWorkers
{
    public class RestoreWorker : WorkerBase
    {
        public override string Name => $"RestoreWorker-{RestoreConfig?.DbName}";
        public override IWorkerConfig Config => RestoreConfig;
        public RestoreWorkerConfig RestoreConfig { get; set; } = new RestoreWorkerConfig();
        public RestoreWorkerState RestoreState { get; set; } = new RestoreWorkerState();
        public override IWorkerState State => RestoreState;
        public IRestoreDownload RestoreDownload { get; set; } = new HttpRestoreDonwload();

        protected override void DebugWriteLine(string msg) => Debug.WriteLine($"----> RESTORE {Name}: {msg}");

        public override async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (RestoreConfig.IsAuto)
                {
                    await State.UpdateStateByProcess(() => DownloadNewBackupAsync(cancellationToken));
                    CallHookAsync(Name, RestoreState);
                    await State.UpdateStateByProcess(() => RestoreAsync(cancellationToken));
                    CallHookAsync(Name, RestoreState);
                }

                if (cancellationToken.IsCancellationRequested) break;
                await Task.Delay(RestoreConfig.DelayTime, cancellationToken);
            }
            return true;
        }

        public async Task RestoreAsync(CancellationToken cancellationToken)
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
                using (var dbConnection = new SqlConnection(RestoreConfig.SqlConnectString))
                {
                    var ok = await restore.RestoreAsync(dbConnection, file);
                    if (!ok)
                    {
                        throw new Exception($"[{restore.Name}] Restore file failed on {RestoreState}");
                    }
                    DebugWriteLine($"[{restore.Name}] Success {RestoreState.CurrentVersion}");
                }

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
        public async Task DownloadNewBackupAsync(CancellationToken cancellationToken)
        {
            //check init value: DownloadedVersion
            RestoreState.DownloadedVersion = RestoreState.DownloadedVersion ?? RestoreState.CurrentVersion;

            var counter = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                var version = await RestoreDownload.DownloadFileAsync(RestoreConfig, RestoreState, cancellationToken);
                if (version == null) return;

                //save version for restore
                RestoreConfig.SaveState(new WorkerStateVersionBase
                {
                    CurrentVersion = RestoreState.DownloadedVersion,
                    NextVersion = version,
                });
                RestoreConfig.SaveState(new WorkerStateVersionBase
                {
                    CurrentVersion = version,
                    NextVersion = null,
                });
                DebugWriteLine($"{RestoreState.DownloadedVersion} => {version}");

                //update state
                RestoreState.DownloadedVersion = version;
                RestoreState.CurrentVersion = RestoreState.CurrentVersion ?? version;

                //check limit
                if (RestoreConfig.MaxFileDownload > 0)
                {
                    counter++;
                    if (counter >= RestoreConfig.MaxFileDownload)
                    {
                        return;
                    }
                }

                //sleep
                await Task.Delay(500, cancellationToken);
            }
        }

    }
}
