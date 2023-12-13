namespace SqlSyncDbService.Workers.ManageWorkers
{
    public record GetNewBackupRequest(string DbId)
    {
        public string? CurrentVersion { get; set; }
        public const string router = "/GetNewBackup";
    }

}
