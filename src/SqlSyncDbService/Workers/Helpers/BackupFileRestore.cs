using SqlSyncDbService.Workers.Interfaces;

namespace SqlSyncDbService.Workers.Helpers
{
    public abstract class BackupFileRestore : IFileRestore
    {
        public abstract string Name {get;}

        protected abstract BackupDatabaseBase BackupDatabase { get; }
        public async Task<bool> RestoreAsync(IWorkerConfig workerConfig, string pathFileZip)
        {
            var dir = Path.GetDirectoryName(pathFileZip) ?? "BackupFileRestore";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var tmp = Path.Combine(dir, VersionFactory.Instance.GetNewVersion());
            try
            {
                var sqlConnectString = workerConfig.SqlConnectString ?? throw new ArgumentNullException(workerConfig.SqlConnectString);
                FileRestoreFactory.SaveStreamData(pathFileZip, tmp);
                return await BackupDatabase.RestoreBackupAsync(sqlConnectString, tmp);
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
