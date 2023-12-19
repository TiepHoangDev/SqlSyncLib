using Microsoft.Data.SqlClient;

namespace SqlSyncDbService.Workers.Interfaces
{
    public interface IFileRestore
    {
        public string Name { get; }
        Task<bool> RestoreAsync(SqlConnection sqlConnection, string pathFileZip);
    }
}
