using System.Text.Json;

namespace SqlSyncLib.LogicBase
{
    public abstract record Fileable
    {
        public abstract string FilePath { get; }

        public virtual bool SaveToFile()
        {
            if (FilePath == null) throw new Exception("Please set FilePath");
            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(FilePath, json);
            return File.Exists(FilePath);
        }

        public static T? LoadFromFile<T>(string FilePath) where T : Fileable
        {
            if (!File.Exists(FilePath)) return default;
            var json = File.ReadAllText(FilePath);
            var obj = JsonSerializer.Deserialize<T>(json);
            return obj;
        }

        public static string CreatePathFile(string filename, params string[] dirs)
        {
            var dirPath = dirs[0];
            foreach (var dir in dirs)
            {
                if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                dirPath = Path.Combine(dirPath, dir);
            }
            return Path.Combine(dirPath, filename);
        }
    }

}
