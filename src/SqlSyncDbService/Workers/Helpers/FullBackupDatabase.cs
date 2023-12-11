namespace SqlSyncDbService.Workers.Helpers
{
    public class FullBackupDatabase : BackupDatabaseBase
    {
        protected override string GetQueryBackup(string dbName, string pathFile)
        {
            var query = $" BACKUP DATABASE [{dbName}] TO DISK='{pathFile}' WITH FORMAT; ";
            return query;
        }

        protected override string GetQueryRestore(string dbName, string pathFile)
        {
            var standby = $"{pathFile}.standby";
            var query = $" RESTORE DATABASE [{dbName}] FROM DISK='{pathFile}' WITH REPLACE, STANDBY='{standby}'; ";
            return query;
        }
    }
}
