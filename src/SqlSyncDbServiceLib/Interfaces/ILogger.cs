using System;

namespace SqlSyncDbServiceLib.Interfaces
{
    public interface ILogger
    {
        void LogError(Exception ex, string message);
        void LogError(string message);
    }
}
