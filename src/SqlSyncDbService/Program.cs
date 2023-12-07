var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:5000;");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(x => x.RoutePrefix = "swagger");

app.MapGet("/", () => "hello");

app.Run();
