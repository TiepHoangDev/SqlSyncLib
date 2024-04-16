using SqlSyncDbServiceLib.Helpers.ScriptsDb;
using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using System;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.Helpers.FileBackups
{
    public abstract class BackupFileBackup : FileRestoreFactory, IFileBackup
    {
        public abstract BackupDatabaseBase BackupDatabase { get; protected set; }

        public virtual async Task<bool> BackupAsync(SqlConnection sqlConnection, string pathFileZip)
        {
            var tmpFile = Path.GetDirectoryName(pathFileZip);
            tmpFile = Path.Combine(tmpFile ?? pathFileZip, VersionFactory.Instance.GetNewVersion());

            if (!await BackupDatabase.CreateBackupAsync(sqlConnection, tmpFile))
            {
                throw new Exception("Create backup fail!");
            }

            using (var zip = ZipFile.Open(pathFileZip, ZipArchiveMode.Create))
            {
                using (var data_fs = File.OpenRead(tmpFile))
                {
                    AppendData(zip, data_fs);
                }
                AppendHeader(zip);
            }

            File.Delete(tmpFile);

            return true;
        }
    }
}
