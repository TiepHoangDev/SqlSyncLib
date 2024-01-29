using SqlSyncDbServiceLib;
using SqlSyncDbServiceLib.Interfaces;

namespace SqlSyncDbService.Models
{
    public class ConfigSqlSyncDbServiceLibDI : IConfigSqlSyncDbServiceLibDI
    {
        public ISqlSyncDbServiceLibLogger GetISqlSyncDbServiceLibLogger(IServiceProvider provider)
        {
            return provider.GetRequiredService<SqlSyncDbServiceLibLogger>();
        }
    }
}
