﻿using System.Data.SqlClient;
using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using SqlSyncDbServiceLib.Helpers.FileBackups;
using SqlSyncDbServiceLib.Helpers.ScriptsDb;

namespace SqlSyncDbServiceLib.Helpers.FileRestores
{
    /// <summary>
    /// abstract class. Restore database from pathFileZip
    /// </summary>
    public abstract class BackupFileRestore : IFileRestore
    {
        public abstract string Name { get; }

        protected abstract BackupDatabaseBase BackupDatabase { get; }
        public async Task<bool> RestoreAsync(SqlConnection sqlConnection, string pathFileZip)
        {
            var dir = Path.GetDirectoryName(pathFileZip) ?? "BackupFileRestore";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var tmp = Path.Combine(dir, VersionFactory.Instance.GetNewVersion());
            try
            {
                var header = FileRestoreFactory.GetHeaderFile(pathFileZip);
                var minVersion = header?.WorkerState.MinVersion
                    ?? throw new ArgumentNullException(header?.WorkerState.MinVersion, $"File not have {header?.WorkerState.MinVersion}, not valid file for restore. {pathFileZip}");

                FileRestoreFactory.SaveFileZip(pathFileZip, tmp);
                return await BackupDatabase.RestoreBackupAsync(sqlConnection, tmp, minVersion);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (File.Exists(tmp)) File.Delete(tmp);
            }
        }
    }
}
