using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.ObjectTranfer.Interfaces
{
    public interface IFileRestore
    {
        string Name { get; }
        Task<bool> RestoreAsync(SqlConnection sqlConnection, string pathFileZip);
    }
}
