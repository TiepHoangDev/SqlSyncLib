using Microsoft.VisualBasic;
using SqlSyncDbService.Workers.Interfaces;
using System;
using System.Diagnostics;

namespace SqlSyncDbService.Workers.Helpers
{
    public abstract record WorkerStateBase : IWorkerState
    {
        public bool? IsSuccess { get; private set; }
        public string? Message { get; private set; }
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
