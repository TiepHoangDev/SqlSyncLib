using System;

namespace SqlSyncDbServiceLib.ObjectTranfer.Interfaces
{
    public interface IConfigSqlSyncDbServiceLibDI
    {
        IDbServiceLibLogger GetISqlSyncDbServiceLibLogger(IServiceProvider provider);
        ILoaderConfig GetILoaderConfig(IServiceProvider provider);
    }

}
