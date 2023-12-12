namespace SqlSyncDbService.Workers.ManageWorkers
{
    public record GetNewBackupRequest(string dbId)
    {
        public string? currentVersion { get; set; }
        public const string router = "/GetNewBackup";
    }

}
