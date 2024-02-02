using Microsoft.AspNetCore.Mvc;
using SqlSyncDbServiceLib.ObjectTranfer;
using SqlSyncDbServiceLib.ObjectTranfer.Instances;
using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;

namespace SqlSyncDbService.Controllers
{
    [ApiController, Route("ManageWorker")]
    public class ManageWorkerController : ControllerBase, IManageWorkerLogic
    {
        private readonly IManageWorkerLogic _manageWorkerLogic;

        public ManageWorkerController(IManageWorkerLogic manageWorkerLogic)
        {
            _manageWorkerLogic = manageWorkerLogic;
        }

        [HttpPost, Route("[action]")]
        public List<IWorker> AddBackupWorker(BackupWorkerConfig config)
            => _manageWorkerLogic.AddBackupWorker(config);

        [HttpPost, Route("[action]")]
        public List<IWorker> AddRestoreWorker(RestoreWorkerConfig config)
            => _manageWorkerLogic.AddRestoreWorker(config);

        [HttpPost, Route("[action]")]
        public IActionResult GetNewBackupApi(GetNewBackupRequest getFileBackup)
        {
            var info = GetNewBackup(getFileBackup);
            if (info == null) return NoContent();
            return File(info.FileStream, "application/octet-stream", info.Version, true);
        }

        [NonAction]
        public GetNewBackupResponse GetNewBackup(GetNewBackupRequest getFileBackup)
            => _manageWorkerLogic.GetNewBackup(getFileBackup);

        [HttpPost, Route("[action]")]
        public List<IWorker> GetWorkers(List<string> ids)
            => _manageWorkerLogic.GetWorkers(ids);

        [HttpPost, Route("[action]")]
        public bool RemoveWorker(string id)
            => _manageWorkerLogic.RemoveWorker(id);
    }
}
