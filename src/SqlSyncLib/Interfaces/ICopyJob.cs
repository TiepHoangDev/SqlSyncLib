namespace SqlSyncLib.Interfaces
{
    public interface ICopyJob : IJobSync
    {
        Task<bool> PullMetadataAsync(IItemSync itemSyncRequest);
        Task PushMetadataAsync(IItemSync itemSync);
    }
}