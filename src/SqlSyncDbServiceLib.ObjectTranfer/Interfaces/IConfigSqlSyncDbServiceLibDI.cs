using System;

namespace SqlSyncDbServiceLib.ObjectTranfer.Interfaces
{
    public interface IConfigSqlSyncDbServiceLibDI
    {
        ISqlSyncDbServiceLibLogger GetISqlSyncDbServiceLibLogger(IServiceProvider provider);
        ILoaderConfig GetILoaderConfig(IServiceProvider provider);
    }

}
