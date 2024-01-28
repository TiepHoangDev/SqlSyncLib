using Microsoft.VisualBasic;
using SqlSyncDbServiceLib.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.Helpers
{
    public abstract class WorkerStateBase : IWorkerState
    {
        public bool? IsSuccess { get; private set; }
        public string Message { get; private set; }
        public DateTime? LastRun { get; private set; }

        public async Task UpdateStateByProcess(Func<Task> process)
        {
            Message = "Running...";
            try
            {
                await process.Invoke();
                IsSuccess = true;
                Message = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("===========Exception==========");
                Debug.WriteLine(ex);
                Debug.WriteLine("===========Exception==========");
                IsSuccess = false;
                Message = ex.Message;
            }
            finally
            {
                LastRun = DateTime.Now;
            }
        }

    }
}
