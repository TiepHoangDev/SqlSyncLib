namespace SqlSyncLib.Interfaces
{
    public interface IRestoreJob : IJobSync
    {
        Task<bool> RestoreAsync(IItemSync itemSync);
    }
}