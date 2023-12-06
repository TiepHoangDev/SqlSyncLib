using SqlSyncLib.Interfaces;
using SqlSyncLib.LogicBase;

namespace SqlSyncLib.CopyLogic
{
    public class CopyJob : BaseJob, ICopyJob
    {
        public Task PushMetadataAsync(IItemSync itemSync)
        {

        }

        public Task<bool> PullMetadataAsync(IItemSync itemSyncRequest)
        {

        }
    }
}
