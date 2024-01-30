using System.IO;
using System.Text.Json;

namespace SqlSyncDbServiceLib.Helpers
{
    public class WorkerStateVersionBase : WorkerStateBase
    {
        public virtual string CurrentVersion { get; set; }
        public virtual string NextVersion { get; set; }

        public virtual bool SaveState(string pathFile)
        {
            if (CurrentVersion == null) return false;
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(pathFile, json);
            return true;
        }

        public virtual string GetNextVersion<T>(string dir, string currentVersion) where T : WorkerStateVersionBase
        {
            var state = GetStateByVersion<T>(GetFilePathState(dir, currentVersion, true));
            return state?.NextVersion;
        }

        public static string GetFilePathState(string dir, string version, bool skipCreateDir = false)
        {
            if (!skipCreateDir && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"{version}.json");
        }

        public static T GetStateByVersion<T>(string filePath) where T : WorkerStateVersionBase
        {
            if (!File.Exists(filePath)) return default;

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }

    }
}
