using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlSyncDbServiceLib.Interfaces;
using SqlSyncDbServiceLib.ManageWorkers;
using System.Reflection;

namespace SqlSyncDbServiceLib
{
    public static partial class SqlSyncDbServiceLibDI
    {
        public static IServiceCollection ConfigSqlSyncDbServiceLibDIDefault(this IServiceCollection services, IConfigSqlSyncDbServiceLibDI configSqlSync)
        {
            services.AddSingleton(configSqlSync.GetISqlSyncDbServiceLibLogger);
            services.AddSingleton<IManageWorker, ManageWorker>();
            services.AddScoped<IManageWorkerLogic, ManageWorkerLogic>();
            return services;
        }
    }
}
