using System.IO;

namespace SqlSyncDbServiceLib.ManageWorkers
{
    public class GetNewBackupResponse
    {
        public Stream FileStream { get; set; }
        public string Version { get; set; }
    }

}
