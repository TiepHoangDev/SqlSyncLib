using SqlSyncDbService.Workers.Interfaces;

namespace SqlSyncDbService.Workers.Helpers
{
    public abstract class BackupFileRestore : IFileRestore
    {
        protected abstract BackupDatabaseBase BackupDatabase { get; }
        public async Task<bool> RestoreAsync(IWorkerConfig workerConfig, string pathFileZip)
        {
            var sqlConnectString = workerConfig.SqlConnectString ?? throw new ArgumentNullException(workerConfig.SqlConnectString);
            var tmp = Path.GetTempFileName();
            using var file = new FileStream(tmp, FileMode.Create);
            var fs = FileRestoreFactory.GetStreamData(pathFileZip);
            fs.CopyTo(file);
            return await BackupDatabase.RestoreBackupAsync(sqlConnectString, tmp);
        }
    }
}
