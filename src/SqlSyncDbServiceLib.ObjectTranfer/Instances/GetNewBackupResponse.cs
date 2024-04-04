using System.IO;

namespace SqlSyncDbServiceLib.ObjectTranfer
{
    public class GetNewBackupResponse
    {
        public Stream FileStream { get; set; }
        public string Version { get; set; }
    }

}
