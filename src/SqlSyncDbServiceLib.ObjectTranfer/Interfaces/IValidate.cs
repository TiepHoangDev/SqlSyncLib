using System.Threading;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.ObjectTranfer.Interfaces
{
    public interface IValidate
    {
        Task ValidateSettingAsync(CancellationToken cancellationToken);
    }
}
