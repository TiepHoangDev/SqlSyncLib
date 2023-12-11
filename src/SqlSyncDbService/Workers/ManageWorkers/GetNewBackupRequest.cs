namespace SqlSyncDbService.Workers.ManageWorkers
{
    public record GetNewBackupRequest(string dbId)
    {
        public string? version { get; set; }
        public const string router = "GetNewBackup";
    }

}
