using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.ObjectTranfer.Interfaces
{
    public interface IWorkerHook
    {
        string Name { get; }
        Task PostData(string name, object data);
    }
}
