using SqlSyncLib.Interfaces;
using SqlSyncLib.LogicBase;

namespace SqlSyncLib.LogicRestore
{
    public class RestoreJob : BaseJob, IRestoreJob
    {
        public Task<bool> RestoreAsync(IItemSync itemSync)
        {
            return itemSync.ApplyAsync();
        }
    }

}
