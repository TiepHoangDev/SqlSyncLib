namespace SqlSyncDbService.Workers.Interfaces
{
    public interface IFileRestore
    {
        public string Name { get; }
        Task<bool> RestoreAsync(IWorkerConfig workerConfig, string pathFileZip);
    }
}
