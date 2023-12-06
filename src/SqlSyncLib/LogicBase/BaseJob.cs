using SqlSyncLib.Interfaces;

namespace SqlSyncLib.LogicBase
{
    public abstract class BaseJob : IJobSync
    {
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
