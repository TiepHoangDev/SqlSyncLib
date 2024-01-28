using System.IO.Compression;
using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Data.SqlClient;
using SqlSyncDbServiceLib.Interfaces;

namespace SqlSyncDbServiceLib.Helpers
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
