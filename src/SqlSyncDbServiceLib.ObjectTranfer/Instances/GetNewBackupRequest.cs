namespace SqlSyncDbServiceLib.ObjectTranfer
{
    public class GetNewBackupRequest
    {
        public const string router = "/GetNewBackup";
        public string IdBackupWorker { get; set; }
        public string CurrentVersion { get; set; }
    }

}
