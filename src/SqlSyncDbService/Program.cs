using Microsoft.AspNetCore.Antiforgery;
using SqlSyncDbService.Services;
using SqlSyncDbService.Workers.Interfaces;
using SqlSyncDbService.Workers.ManageWorkers;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:5000;");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddSingleton<IManageWorker, ManageWorker>();
builder.Services.AddHostedService<ManageWorkerService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseDeveloperExceptionPage();
app.MapControllers();

app.MapGet("/", () => DateTime.Now);

app.UseRouting();
app.UseEndpoints(configure =>
{
    var input = new MapEndPointInput(configure);
    app.Services.GetRequiredService<IManageWorker>().MapEndPoint(input);
});


app.Run();
