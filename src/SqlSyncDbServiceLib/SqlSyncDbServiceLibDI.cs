using Microsoft.Extensions.DependencyInjection;
using SqlSyncDbServiceLib.ManageWorkers;
using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;

namespace SqlSyncDbServiceLib
{
    public static partial class SqlSyncDbServiceLibDI
    {
        public static IServiceCollection ConfigSqlSyncDbServiceLibDIDefault(this IServiceCollection services, IConfigSqlSyncDbServiceLibDI configSqlSync)
        {
            services.AddSingleton(configSqlSync.GetISqlSyncDbServiceLibLogger);
            services.AddSingleton(configSqlSync.GetILoaderConfig);
            services.AddSingleton<IManageWorker, ManageWorker>();
            services.AddScoped<IManageWorkerLogic, ManageWorkerLogic>();
            return services;
        }
    }
}
