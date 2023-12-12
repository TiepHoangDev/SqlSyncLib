using Microsoft.Data.SqlClient;
using SqlSyncDbService.Workers.Interfaces;
using System.Xml.Linq;

namespace SqlSyncDbService.Workers.RestoreWorkers
{
    public class RestoreWorkerConfig : IWorkerConfig
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? IdBackupWorker { get; set; }
        public TimeSpan DelayTime { get; set; } = TimeSpan.FromSeconds(8);
        public string DbName = "";
        public string? BackupAddress { get; set; }
        public string? PathFolder { get; set; } = "./data/DbName/restores";
        private string? sqlConnectString;
        public string? SqlConnectString
        {
            get => sqlConnectString;
            set
            {
                sqlConnectString = value;
                DbName = new SqlConnectionStringBuilder(sqlConnectString).InitialCatalog;
                var pathFolder = Path.Combine("./data/", DbName, "restores");
                PathFolder = Path.GetFullPath(pathFolder);
            }
        }

        public string GetFilePath(string version, bool ensureFolder = true)
        {
            if (string.IsNullOrWhiteSpace(PathFolder))
            {
                throw new ArgumentNullException(nameof(PathFolder));
            }
            var path = Path.Combine(PathFolder, $"{version}.syncdb");
            if (ensureFolder && !Directory.Exists(PathFolder)) Directory.CreateDirectory(PathFolder);
            return Path.GetFullPath(path);
        }
    }
}
