using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using SqlSyncDbService.Workers.Interfaces;
using System.Diagnostics;
using System.Text.Json;

namespace SqlSyncLib.Workers.BackupWorkers
{
    public class BackupWorkerConfig : IWorkerConfig
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public TimeSpan DelayTime { get; set; } = TimeSpan.FromSeconds(8);
        public TimeOnly ResetAtTime { get; set; } = new TimeOnly(4, 0, 0);
        public DayOfWeek? ResetAtDay { get; set; } = DayOfWeek.Saturday;
        public DateTime? LastRun { get; set; }
        public string PathFileState { get; private set; } = "./data/DbName/states";
        public string PathFolder { get; private set; } = "./data/DbName/backups";
        public string DbName = "";
        public string? SqlConnectString
        {
            get => sqlConnectString;
            set
            {
                sqlConnectString = value;
                DbName = new SqlConnectionStringBuilder(sqlConnectString).InitialCatalog;
                var pathFolder = Path.Combine("./data/", DbName, "backups");
                PathFolder = Path.GetFullPath(pathFolder);
                PathFileState = Path.Combine("./data/", DbName, "states");
            }
        }
        private string? sqlConnectString;

        public bool IsReset(DateTime now)
        {
            if (ResetAtDay.HasValue && now.DayOfWeek != ResetAtDay) return false;
            if (now.TimeOfDay - ResetAtTime.ToTimeSpan() > DelayTime) return false;
            return true;
        }

        public bool IsExistBackupFull(string minVersion)
        {
            return File.Exists(GetPathBackupFull(minVersion));
        }

        public string GetPathBackupFull(string version) => GetPathFile(version, version);

        public string GetPathFile(string minVersion, string version)
        {
            var path = Path.Combine(PathFolder, minVersion, $"{DbName}.{version}.syncdb");
            return Path.GetFullPath(path);
        }

        public void DeleteMinVersion(string minVersion)
        {
            var path = GetPathBackupFull(minVersion);
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) return;
            var files = Directory.GetFiles(dir);

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
            Directory.Delete(dir);
        }

        public void DeleteVersion(string minVersion, string version)
        {
            var path = GetPathFile(minVersion, version);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        #region SAVE/LOAD STATES


        public bool SaveState(BackupWorkerState state)
        {
            if (state.NextVersion == null) return false;
            if (state.CurrentVersion == null) return false;
            var json = JsonSerializer.Serialize(state);
            var filePath = GetFilePathState(state.CurrentVersion);
            File.WriteAllText(filePath, json);
            return true;
        }

        public BackupWorkerState? GetStateByVersion(string version)
        {
            var filePath = GetFilePathState(version, true);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<BackupWorkerState>(json);
            }
            return default;
        }

        public string GetFilePathState(string version, bool skipCreateDir = false)
        {
            if (!skipCreateDir && !Directory.Exists(PathFileState)) Directory.CreateDirectory(PathFileState);
            return Path.Combine(PathFileState, $"{version}.json");
        }


        #endregion

        public string GetNextVersion(string? currentVersion, BackupWorkerState currentState)
        {
            if (currentVersion == null) return currentState.MinVersion;
            if (currentVersion.CompareTo(currentState.MinVersion) < 0) return currentState.MinVersion;
            if (currentVersion.CompareTo(currentState.CurrentVersion) > 0)
                throw new Exception($"version={currentVersion} is not valid. CurrentVersion is {currentState.CurrentVersion}");
            var state = GetStateByVersion(currentVersion);
            var nextState = state?.CurrentVersion ?? throw new Exception($"Can not find next version.");
            if (state.MinVersion != currentState.MinVersion) throw new Exception($"Next version is not valid with minversion={currentState.MinVersion}.");
            return nextState;
        }

    }
}
