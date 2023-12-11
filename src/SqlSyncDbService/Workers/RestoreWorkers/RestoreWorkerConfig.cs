using SqlSyncDbService.Workers.Interfaces;

namespace SqlSyncDbService.Workers.RestoreWorkers
{
    public class RestoreWorkerConfig : IWorkerConfig
    {
        public TimeSpan DelayTime { get; set; } = TimeSpan.FromSeconds(8);
        public DateTime? LastRun { get; set; }
        public string dbId { get; set; } = "";
        public string? SqlConnectString { get; set; }
        public string? BackupAddress { get; set; }
        public string? PathFolder { get; set; } = "./restore";

        public string GetFilePath(string? version)
        {
            if (string.IsNullOrWhiteSpace(PathFolder))
            {
                throw new ArgumentNullException(nameof(PathFolder));
            }
            var filename = version ?? "unknow";
            return Path.Combine(PathFolder, $"{filename}.bak");
        }
    }
}
