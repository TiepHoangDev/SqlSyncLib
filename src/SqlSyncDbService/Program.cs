using SqlSyncDbService.Models;
using SqlSyncDbService.Services;
using SqlSyncDbServiceLib.Interfaces;
using SqlSyncDbServiceLib.ManageWorkers;

var builder = WebApplication.CreateBuilder(args);

//logging
//https://github.com/nreco/logging
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddFile("{0}", fileLoggerOpts =>
    {
        fileLoggerOpts.FormatLogFileName = fName =>
        {
            return $"logs/{DateTime.UtcNow:yyyy/MM/dd}.log.txt";
        };
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddScoped<SqlSyncDbServiceLib.Interfaces.ILogger, WorkerLogger>();
builder.Services.AddSingleton<IManageWorker, ManageWorker>();
builder.Services.AddScoped<IManageWorkerLogic, ManageWorkerLogic>();
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

var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknow";
Console.WriteLine($"version = {version}");

app.MapGet("/", () => Results.Redirect("/swagger/index.html"));
app.MapGet("/v", () => version);

app.Run();

