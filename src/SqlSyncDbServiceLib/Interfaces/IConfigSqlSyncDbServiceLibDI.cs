using SqlSyncDbServiceLib.Interfaces;
using System;

namespace SqlSyncDbServiceLib
{
    public interface IConfigSqlSyncDbServiceLibDI
    {
        ISqlSyncDbServiceLibLogger GetISqlSyncDbServiceLibLogger(IServiceProvider provider);
    }
}
