using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.Interfaces
{
    public interface IWorkerHook
    {
        string Name { get; }
        Task PostData(string name, object data);
    }
}
