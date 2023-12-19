using Microsoft.Data.SqlClient;

namespace SqlSyncDbService.Workers.Interfaces
{
    public interface IFileBackup
    {
        Task<bool> BackupAsync(SqlConnection sqlConnection, string pathFileZip);
    }
}
