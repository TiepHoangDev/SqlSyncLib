using FastQueryLib;
using Microsoft.Data.SqlClient;
using SqlSyncLib.LogicRestore;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace SqlSyncLib.Workers.BackupWorkers
{
    public class BackupWorker : IWorker
    {
        public string Name => $"BackupWorker-{BackupConfig.DbName}";
        public List<IWorkerHook> Hooks { get; } = new List<IWorkerHook> { };
        public IWorkerConfig Config => BackupConfig;
        public IWorkerApi Api => BackupApis;
        public IWorkerState State => BackupState;

        public BackupWorkerConfig BackupConfig { get; } = new BackupWorkerConfig();
        public BackupWorkerState BackupState { get; } = new BackupWorkerState();
        public BackupWorkerApi BackupApis { get; } = new BackupWorkerApi();


        public async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var sqlConnectString = BackupConfig.SqlConnectString ?? throw new ArgumentNullException(nameof(BackupConfig.SqlConnectString));

                    var now = DateTime.Now;
                    BackupConfig.LastRun = now;
                    var isReset = BackupState.MinVersion == null || BackupConfig.IsReset(now);

                    var currentVersion = VersionFactory.Instance.GetNewVersion();
                    var dir = isReset ? currentVersion : (BackupState.MinVersion ?? currentVersion);
                    dir = Path.Combine(BackupConfig.PathFolder, dir);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    var pathFile = Path.Combine(dir, $"{BackupConfig.DbName}.{currentVersion}.bak");
                    var success = false;

                    if (isReset)
                    {
                        success = await new BackupWorkerApi().BackupFull(sqlConnectString, pathFile);
                        if (success)
                        {
                            if (BackupState.MinVersion != null)
                            {
                                // delete old version
                                var dirOld = Path.Combine(BackupConfig.PathFolder, BackupState.MinVersion);
                                var files = Directory.GetFiles(dirOld);
                                foreach (var file in files)
                                {
                                    try
                                    {
                                        File.Delete(file);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine(ex);
                                    }
                                }
                                Directory.Delete(dirOld);
                            }
                            BackupState.MinVersion = currentVersion;
                        }
                    }
                    else
                    {
                        success = await new BackupWorkerApi().BackupLog(sqlConnectString, pathFile);
                    }

                    if (success)
                    {
                        BackupState.CurrentVersion = currentVersion;
                        BackupState.DbId = BackupConfig.DbName;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                if (cancellationToken.IsCancellationRequested) break;
                await Task.Delay(BackupConfig.DelayTime, cancellationToken);
            }
            return true;
        }
    }

    public record BackupWorkerState() : IWorkerState
    {
        public string? MinVersion { get; set; }
        public string DbId { get; set; } = "";
        public string? CurrentVersion { get; set; }
    }

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
    }

    public class BackupWorkerApi : IWorkerApi
    {
        public virtual async Task<bool> BackupFull(string sqlConnectString, string pathFile)
        {
            return await new FullBackupDatabase().CreateBackupAsync(sqlConnectString, pathFile);
        }

        public virtual async Task<bool> BackupLog(string sqlConnectString, string pathFile)
        {
            return await new LogBackupDatabase().CreateBackupAsync(sqlConnectString, pathFile);
        }
    }

    public abstract class BackupDatabaseBase
    {
        public virtual async Task<bool> ApplyAsync(string sqlConnectString, string query)
        {
            using var conn = new SqlConnection(sqlConnectString);
            var dbName = conn.Database;
            using var master = conn.NewOpenConnectToDatabase("master");
            using var restoreJob = await master.CreateFastQuery().WithQuery(query).ExecuteNumberOfRowsAsync();
            return await master.CheckDatabaseExistsAsync(dbName);
        }

        public virtual async Task<bool> CreateBackupAsync(string sqlConnectString, string pathFile)
        {
            var dbName = new SqlConnectionStringBuilder(sqlConnectString).InitialCatalog;
            var query = GetQueryBackup(dbName, pathFile);
            var backupSuccess = await ApplyAsync(sqlConnectString, query);
            return backupSuccess;
        }

        protected abstract string GetQueryBackup(string dbName, string pathFile);

        public virtual async Task<bool> RestoreBackupAsync(string sqlConnectString, string pathFile)
        {
            var dbName = new SqlConnectionStringBuilder(sqlConnectString).InitialCatalog;
            var query = GetQueryRestore(dbName, pathFile);
            var backupSuccess = await ApplyAsync(sqlConnectString, query);
            return backupSuccess;
        }

        protected abstract string GetQueryRestore(string dbName, string pathFile);
    }

    public class FullBackupDatabase : BackupDatabaseBase
    {
        protected override string GetQueryBackup(string dbName, string pathFile)
        {
            var query = $" BACKUP DATABASE [{dbName}] TO DISK='{pathFile}' WITH FORMAT; ";
            return query;
        }

        protected override string GetQueryRestore(string dbName, string pathFile)
        {
            var standby = $"{pathFile}.standby";
            var query = $" RESTORE DATABASE [{dbName}] FROM DISK='{pathFile}' WITH REPLACE, STANDBY='{standby}'; ";
            return query;
        }
    }

    public class LogBackupDatabase : BackupDatabaseBase
    {
        protected override string GetQueryBackup(string dbName, string pathFile)
        {
            var query = $" BACKUP LOG [{dbName}] TO DISK='{pathFile}' WITH FORMAT; ";
            return query;
        }

        protected override string GetQueryRestore(string dbName, string pathFile)
        {
            var standby = $"{pathFile}.standby";
            var query = $" RESTORE LOG [{dbName}] FROM DISK='{pathFile}' WITH REPLACE, STANDBY='{standby}'; ";
            return query;
        }
    }
}
