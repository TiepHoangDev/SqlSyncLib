namespace SqlSyncLib.Workers.BackupWorkers
{
    public record BackupWorkerState()
    {
        public string MinVersion { get; set; } = "no_min_version";
        public string Id { get; set; } = "no_db_id";
        public string? CurrentVersion { get; set; }
    }
}
