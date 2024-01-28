using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.Interfaces
{
    public interface IFileBackup
    {
        Task<bool> BackupAsync(SqlConnection sqlConnection, string pathFileZip);
    }
}
