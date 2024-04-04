﻿using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using System;
using System.Diagnostics;
using System.Threading;
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

        public virtual string SuccessString => IsSuccess ?? false ? "OK" : "FAILED";

        public override string ToString()
        {
            return $"[{LastRun:HH:mm:ss.fff}] [{SuccessString}]{Message}";
        }
    }
}
