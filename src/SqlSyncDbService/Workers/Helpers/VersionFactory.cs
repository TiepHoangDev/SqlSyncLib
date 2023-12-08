namespace SqlSyncLib.LogicBase
{
    public class VersionFactory
    {
        public static readonly VersionFactory Instance = new VersionFactory();

        public string GetNewVersion()
        {
            return DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss");
        }
    }
}
