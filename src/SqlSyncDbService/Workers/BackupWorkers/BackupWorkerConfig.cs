using Microsoft.Data.SqlClient;
using SqlSyncDbService.Workers.Interfaces;

namespace SqlSyncLib.Workers.BackupWorkers
{
    public class BackupWorkerConfig : IWorkerConfig
    {
        public TimeSpan DelayTime { get; set; } = TimeSpan.FromSeconds(8);
        public TimeOnly ResetAtTime { get; set; } = new TimeOnly(4, 0, 0);
        public DayOfWeek? ResetAtDay { get; set; } = DayOfWeek.Saturday;
        public DateTime? LastRun { get; set; }
        public string? SqlConnectString
        {
            get => sqlConnectString;
            set
            {
                sqlConnectString = value;
                DbName = new SqlConnectionStringBuilder(sqlConnectString).InitialCatalog;
            }
        }
        public string PathFolder { get; set; } = "./backup";
        public string DbName = "";
        private string? sqlConnectString;

        public bool IsReset(DateTime now)
        {
            if (ResetAtDay.HasValue && now.DayOfWeek != ResetAtDay) return false;
            if (now.TimeOfDay - ResetAtTime.ToTimeSpan() > DelayTime) return false;
            return true;
        }

        public string GetPathFile(string minVersion, string version)
        {
            return Path.Combine(PathFolder, minVersion, $"{DbName}.{version}.bak");
        }
    }
}
