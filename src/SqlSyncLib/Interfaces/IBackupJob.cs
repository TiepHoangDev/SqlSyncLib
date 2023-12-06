namespace SqlSyncLib.Interfaces
{
    public interface IBackupJob : IJobSync
    {
        Task<IItemSync> CreateBackupFullAsync();
        Task<IItemSync> CreateBackupLogAsync();
    }
}