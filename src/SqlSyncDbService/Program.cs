using SqlSyncDbService.Services;
using SqlSyncDbService.Workers.Interfaces;
using SqlSyncDbService.Workers.ManageWorkers;
using SqlSyncDbService.Workers.RestoreWorkers;
using SqlSyncLib.Workers.BackupWorkers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:5000;");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddSingleton<IManageWorker, ManageWorker>();
builder.Services.AddHostedService<ManageWorkerService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DisplayRequestDuration();
    options.EnableTryItOutByDefault();
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});
app.UseDeveloperExceptionPage();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

#if DEBUG
_ = Task.Run(async () =>
{
#if true
    var server = ".";
#else
    var server = ".\\SQLEXPRESS";
#endif

    await Task.Delay(TimeSpan.FromSeconds(5));

    var manage = app.Services.GetRequiredService<IManageWorker>();
    var backup = new BackupWorker
    {
        BackupConfig = new BackupWorkerConfig
        {
            SqlConnectString = FastQueryLib.SqlServerExecuterHelper.CreateConnectionString(server, "A").ToString(),
            DelayTime = TimeSpan.FromSeconds(5)
        }
    };
    manage.AddWorker(backup);
    manage.AddWorker(new RestoreWorker
    {
        RestoreConfig = new RestoreWorkerConfig
        {
            SqlConnectString = FastQueryLib.SqlServerExecuterHelper.CreateConnectionString(server, "A_copy").ToString(),
            BackupAddress = "http://localhost:5000/",
            IdBackupWorker = backup.Config.Id,
            DelayTime = TimeSpan.FromSeconds(5)
        },
    });

});
#endif

app.Run();

