namespace SqlSyncDbServiceLib.ObjectTranfer
{
    public class GetNewBackupRequest
    {
        public const string router = "/GetNewBackup";
        public string DbId { get; set; }
        public string CurrentVersion { get; set; }
    }

}
