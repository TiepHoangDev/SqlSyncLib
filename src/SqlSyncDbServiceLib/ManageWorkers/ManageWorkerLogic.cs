using SqlSyncDbServiceLib.BackupWorkers;
using SqlSyncDbServiceLib.Interfaces;
using SqlSyncDbServiceLib.RestoreWorkers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SqlSyncDbServiceLib.ManageWorkers
{
    public class ManageWorkerLogic : IManageWorkerLogic
    {
        readonly ILogger logger;
        private readonly IManageWorker _manageWorker;

        public ManageWorkerLogic(IManageWorker manageWorker, ILogger<ManageWorkerLogic> logger)
        {
            _manageWorker = manageWorker;
            this.logger = logger;
        }

        public List<IWorker> GetWorkers(List<string> ids = null)
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
            var workers = GetWorkers(new List<string> { getFileBackup.DbId });
            var worker = workers.FirstOrDefault();
            if (worker is BackupWorker backup)
            {
                var filePath = backup.GetFileBackup(getFileBackup.CurrentVersion, out var version);
                if (filePath != null && File.Exists(filePath))
                {
                    var fs = File.OpenRead(filePath);
                    return new GetNewBackupResponse
                    {
                        FileStream = fs,
                        Version = version
                    };
                }
                return default;
            }
            throw new Exception($"Not exist BackupWorker with id = {getFileBackup.DbId}");
        }

        protected virtual List<IWorker> ApiAddWorker(IWorker worker)
        {
            if (_manageWorker.AddWorker(worker))
            {
                return GetWorkers();
            }
            return null;
        }

        public virtual List<IWorker> AddBackupWorker(BackupWorkerConfig config)
        {
            var worker = new BackupWorker(logger)
            {
                BackupConfig = config,
            };
            return ApiAddWorker(worker);
        }

        public virtual List<IWorker> AddRestoreWorker(RestoreWorkerConfig config)
        {
            var worker = new RestoreWorker(logger)
            {
                RestoreConfig = config,
            };
            return ApiAddWorker(worker);
        }
    }
}
