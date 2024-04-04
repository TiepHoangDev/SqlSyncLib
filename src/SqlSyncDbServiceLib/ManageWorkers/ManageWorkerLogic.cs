using SqlSyncDbServiceLib.BackupWorkers;
using SqlSyncDbServiceLib.ObjectTranfer;
using SqlSyncDbServiceLib.ObjectTranfer.Instances;
using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using SqlSyncDbServiceLib.RestoreWorkers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SqlSyncDbServiceLib.ManageWorkers
{
    public class ManageWorkerLogic : IManageWorkerLogic
    {
        private readonly IManageWorker _manageWorker;

        public ManageWorkerLogic(IManageWorker manageWorker)
        {
            _manageWorker = manageWorker;
        }

        public List<IWorker> GetWorkers(List<string> ids)
        {
            return _manageWorker.GetWorkers(ids);
        }

        public bool RemoveWorker(string id)
        {
            return _manageWorker.RemoveWorker(q => q.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Route(GetNewBackupRequest.router)
        /// </summary>
        /// <param name="getFileBackup"></param>
        /// <returns></returns>
        public GetNewBackupResponse GetNewBackup(GetNewBackupRequest getFileBackup)
        {
            if (string.IsNullOrWhiteSpace(getFileBackup?.IdBackupWorker))
            {
                throw new Exception($"Request missing value of {nameof(GetNewBackupRequest.IdBackupWorker)}");
            }
            var workers = GetWorkers(new List<string> { getFileBackup.IdBackupWorker });
            var worker = workers.FirstOrDefault();
            if (worker is BackupWorker backup)
            {
                var filePath = backup.GetFileBackup(getFileBackup.CurrentVersion, out var version);
                if (filePath != null && File.Exists(filePath))
                {
                    var fs = File.OpenRead(filePath);
                    fs.Seek(0, SeekOrigin.Begin);
                    return new GetNewBackupResponse
                    {
                        FileStream = fs,
                        Version = version
                    };
                }
                return default;
            }
            throw new Exception($"Not exist BackupWorker with id = {getFileBackup.IdBackupWorker}");
        }

        protected virtual List<IWorker> ApiAddWorker(IWorker worker)
        {
            if (_manageWorker.AddWorker(worker))
            {
                return GetWorkers(null);
            }
            return null;
        }

        public virtual List<IWorker> AddBackupWorker(BackupWorkerConfig config)
        {
            var worker = new BackupWorker
            {
                BackupConfig = config,
            };
            return ApiAddWorker(worker);
        }

        public virtual List<IWorker> AddRestoreWorker(RestoreWorkerConfig config)
        {
            var worker = new RestoreWorker
            {
                RestoreConfig = config,
            };
            return ApiAddWorker(worker);
        }
    }
}
