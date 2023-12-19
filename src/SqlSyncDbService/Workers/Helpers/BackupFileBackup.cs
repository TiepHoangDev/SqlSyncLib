using Microsoft.Data.SqlClient;
using SqlSyncDbService.Workers.Interfaces;
using System.IO.Compression;

namespace SqlSyncDbService.Workers.Helpers
{
    public abstract class BackupFileBackup : FileRestoreFactory, IFileBackup
    {
        protected abstract BackupDatabaseBase BackupDatabase { get; }

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
