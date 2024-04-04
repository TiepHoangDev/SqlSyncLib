using System;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.ObjectTranfer.Interfaces
{
    public interface IWorkerState
    {
        bool? IsSuccess { get; }
        string Message { get; }
        DateTime? LastRun { get; }

        Task UpdateStateByProcess(Func<Task> process);
    }
}
