using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSyncLib.LogicBase;

namespace SqlSyncLib.Interfaces
{
    public interface IItemSync
    {
        SqlSyncMetadata Metadata { get; }
        EnumTypeSync TypeSync { get; }
        Task<bool> ApplyAsync(InputApplyItemSync inputApply);
    }

    public record InputApplyItemSync(string ConnectString, SqlSyncMetadata Metadata)
    {

    };
}
