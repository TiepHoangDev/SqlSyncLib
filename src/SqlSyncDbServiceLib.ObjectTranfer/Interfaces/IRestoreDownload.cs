using SqlSyncDbServiceLib.ObjectTranfer.Instances;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.ObjectTranfer.Interfaces
{
    public interface IRestoreDownload
    {
        Task<string> DownloadFileAsync(RestoreWorkerConfig RestoreConfig, RestoreWorkerState RestoreState, CancellationToken cancellationToken);
    }
}
