namespace SqlSyncLib.Workers.CopyWorkers
{
    public class CopyWorker : IWorker
    {
        public string Name => $"CopyWorker";

        public List<IWorkerHook> Hooks { get; } = new List<IWorkerHook> { };

        public IWorkerConfig Config => CopyConfig;
        public IWorkerApi Api => CopyApi;
        public IWorkerState State => CopyState;

        public CopyWorkerConfig CopyConfig { get; set; } = new CopyWorkerConfig();
        public CopyWorkerApi CopyApi { get; set; } = new CopyWorkerApi();
        public CopyWorkerState CopyState { get; set; } = new CopyWorkerState();


        public Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class CopyWorkerConfig : IWorkerConfig
    {
        public TimeSpan DelayTime { get; set; } = TimeSpan.FromSeconds(8);
        public string PathFolder { get; set; } = "./copy";
    }

    public class CopyWorkerApi : IWorkerApi
    {

    }

    public class CopyWorkerState : IWorkerState
    {
        public string DbId { get; set; } = "";
    }
}
