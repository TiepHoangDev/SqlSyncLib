using SqlSyncDbService.Workers.Interfaces;

namespace SqlSyncDbService.Workers.Helpers
{
    public abstract class BackupFileRestore : IFileRestore
    {
        public abstract string Name { get; }

        protected abstract BackupDatabaseBase BackupDatabase { get; }
        public async Task<bool> RestoreAsync(IWorkerConfig workerConfig, string pathFileZip)
        {
            var dir = Path.GetDirectoryName(pathFileZip) ?? "BackupFileRestore";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var tmp = Path.Combine(dir, VersionFactory.Instance.GetNewVersion());
            try
            {
                var sqlConnectString = workerConfig.SqlConnectString
                    ?? throw new ArgumentNullException(workerConfig.SqlConnectString, $"Please config {nameof(workerConfig.SqlConnectString)} for {nameof(workerConfig)}");
                var header = FileRestoreFactory.GetHeaderFile(pathFileZip);
                var minVersion = header?.WorkerState.MinVersion
                    ?? throw new ArgumentNullException(header?.WorkerState.MinVersion, $"File not have {header?.WorkerState.MinVersion}, not valid file for restore. {pathFileZip}");

                FileRestoreFactory.SaveStreamData(pathFileZip, tmp);
                return await BackupDatabase.RestoreBackupAsync(sqlConnectString, tmp, minVersion);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
#if DEBUG0
                if (File.Exists(tmp)) File.Delete(tmp);
#endif
            }
        }
    }
}
