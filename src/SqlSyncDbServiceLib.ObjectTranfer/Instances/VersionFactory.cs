using System;

namespace SqlSyncDbServiceLib.Helpers
{
    public class VersionFactory
    {
        public static readonly VersionFactory Instance = new VersionFactory();

        public string GetNewVersion()
        {
            var id = Guid.NewGuid().GetHashCode().ToString();
            return $"{DateTime.Now:yyyy.MM.dd-HH.mm.ss.ffff}-{id}";
        }
    }
}
