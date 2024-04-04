using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;

namespace SqlSyncDbService.Models
{
    public class ConfigSqlSyncDbServiceLibDI : IConfigSqlSyncDbServiceLibDI
    {
        public ILoaderConfig GetILoaderConfig(IServiceProvider provider)
        {
            return default;
        }

        public IDbServiceLibLogger GetISqlSyncDbServiceLibLogger(IServiceProvider provider)
        {
            return provider.GetRequiredService<SqlSyncDbServiceLibLogger>();
        }
    }
}
