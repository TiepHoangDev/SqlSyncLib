using Microsoft.AspNetCore.Mvc;
using SqlSyncDbService.Workers.Interfaces;
using SqlSyncDbService.Workers.RestoreWorkers;
using SqlSyncLib.Workers.BackupWorkers;
using System.IO;

namespace SqlSyncDbService.Workers.ManageWorkers
{
    [ApiController, Route("ManageWorker")]
    public class ManageWorkerController : ControllerBase
    {
        private readonly IManageWorker _manageWorker;

        public ManageWorkerController(IManageWorker manageWorker)
        {
            _manageWorker = manageWorker;
        }

        [HttpPost, Route("GetWorkers")]
        public List<IWorker> GetWorkers(List<string>? ids = null)
        {
            return _manageWorker.GetWorkers(ids);
        }

        [HttpPost, Route("RemoveWorker")]
        public bool RemoveWorker(string id)
        {
            return _manageWorker.RemoveWorker(q => q.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
        }

        [HttpPost, Route(GetNewBackupRequest.router)]
        public IActionResult GetNewBackup(GetNewBackupRequest getFileBackup)
        {
            var workers = GetWorkers(new List<string> { getFileBackup.dbId });
            var worker = workers.FirstOrDefault();
            if (worker is BackupWorker backup)
            {
                var nextVersion = backup.GetNextVersion(getFileBackup.currentVersion);
                var filePath = backup.GetFileBackup(nextVersion);
                if (System.IO.File.Exists(filePath))
                {
                    var fs = System.IO.File.OpenRead(filePath);
                    return File(fs, "application/octet-stream", nextVersion);
                }
            }
            return NotFound();
        }

        [NonAction]
        public virtual List<IWorker>? ApiAddWorker(IWorker worker)
        {
            if (_manageWorker.AddWorker(worker))
            {
                return GetWorkers();
            }
            return null;
        }

        [HttpPost, Route("[action]")]
        public virtual List<IWorker>? AddBackupWorker(BackupWorkerConfig config)
        {
            var worker = new BackupWorker
            {
                BackupConfig = config,
            };
            return ApiAddWorker(worker);
        }

        [HttpPost, Route("[action]")]
        public virtual List<IWorker>? AddRestoreWorker(RestoreWorkerConfig config)
        {
            var worker = new RestoreWorker
            {
                RestoreConfig = config,
            };
            return ApiAddWorker(worker);
        }
    }
}
