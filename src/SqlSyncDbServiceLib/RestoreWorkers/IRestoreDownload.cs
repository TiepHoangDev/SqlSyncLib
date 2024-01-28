using System.Threading.Tasks;
using System.Threading;

namespace SqlSyncDbServiceLib.RestoreWorkers
{
    public interface IRestoreDownload
    {
        Task<string> DownloadFileAsync(RestoreWorkerConfig RestoreConfig, RestoreWorkerState RestoreState, CancellationToken cancellationToken);
    }
}
