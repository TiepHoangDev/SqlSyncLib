namespace SqlSyncDbService.Workers.Helpers
{
    public class VersionFactory
    {
        public static readonly VersionFactory Instance = new();

        public string GetNewVersion()
        {
            var id = Guid.NewGuid().GetHashCode().ToString();
            return $"{DateTime.Now:yyyy.MM.dd-HH.mm.ss.ffff}-{id}";
        }
    }
}
