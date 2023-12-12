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

        private string? sqlConnectString;
        public string? SqlConnectString
        {
            get => sqlConnectString;
            set
            {
                sqlConnectString = value;
                DbName = new SqlConnectionStringBuilder(sqlConnectString).InitialCatalog;
                var pathFolder = Path.Combine("./", DbName, "restore");
                PathFolder = Path.GetFullPath(pathFolder);
            }
        }
        public string? BackupAddress { get; set; }
        public string? PathFolder { get; set; } = "./restore";


        public string GetFilePath(string? version, bool ensureFolder = true)
        {
            if (string.IsNullOrWhiteSpace(PathFolder))
            {
                throw new ArgumentNullException(nameof(PathFolder));
            }
            var filename = version ?? "unknow";
            var path = Path.Combine(PathFolder, $"{filename}.syncdb");
            if (ensureFolder && !Directory.Exists(PathFolder)) Directory.CreateDirectory(PathFolder);
            return Path.GetFullPath(path);
        }
    }
}
