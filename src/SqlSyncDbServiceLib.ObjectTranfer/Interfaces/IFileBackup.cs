using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.ObjectTranfer.Interfaces
{
    public interface IFileBackup
    {
        Task<bool> BackupAsync(SqlConnection sqlConnection, string pathFileZip);
    }
}
