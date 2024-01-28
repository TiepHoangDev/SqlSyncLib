using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.Interfaces
{
    public interface IFileRestore
    {
        string Name { get; }
        Task<bool> RestoreAsync(SqlConnection sqlConnection, string pathFileZip);
    }
}
