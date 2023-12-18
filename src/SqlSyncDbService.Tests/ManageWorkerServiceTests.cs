using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FastQueryLib;
using Microsoft.Data.SqlClient;
using Moq;
using SqlSyncDbService.Workers.Interfaces;
using SqlSyncDbService.Workers.ManageWorkers;
using SqlSyncDbService.Workers.RestoreWorkers;
using SqlSyncLib.Workers.BackupWorkers;
using System.Diagnostics;

namespace SqlSyncDbService.Tests;

public class ManageWorkerServiceTests
{
    private CancellationTokenSource _tokenSource;
    private BackupWorker _backup;
    private RestoreWorker _restore;
    private IContainer _container;

    [OneTimeSetUp]
    public async Task Setup()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        _tokenSource = new CancellationTokenSource();

        var server = "localhost";
        var port = 2000;
        var username = "sa";
        var pass = "dev@1234";
        var database = "A";

        //create sql
        _container = new ContainerBuilder()
           .WithImage("mcr.microsoft.com/mssql/server")
           .WithPortBinding(1433, true)
           .WithEnvironment("ACCEPT_EULA", "Y")
           .WithEnvironment("SQLCMDUSER", username)
           .WithEnvironment("SQLCMDPASSWORD", pass)
           .WithEnvironment("MSSQL_SA_PASSWORD", pass)
           .WithVolumeMount("sql_test", "/var/opt/mssql/data")
           .WithWaitStrategy(Wait.ForUnixContainer().UntilContainerIsHealthy())
           .Build();

        await _container.StartAsync(_tokenSource.Token);
        port = _container.GetMappedPublicPort(1433);
        server = $"{_container.Hostname},{port}";

        var conn = SqlServerExecuterHelper.CreateConnectionString(server, database, username, pass).ToString();
        Console.WriteLine(conn);

        if (!await new SqlConnection(conn).CreateFastQuery().CheckDatabaseExistsAsync())
        {
            using var faster = new SqlConnection(conn).CreateFastQuery().UseDatabase("master");
            await faster.WithQuery($@"create database {database};").ExecuteNonQueryAsync();
            await faster.WithQuery($@"use {database}; create table Products ( ID int primary key identity, CreateTime datetime );").ExecuteNonQueryAsync();
        }

        //setup project
        _backup = new BackupWorker
        {
            BackupConfig = new BackupWorkerConfig
            {
                SqlConnectString = SqlServerExecuterHelper.CreateConnectionString(server, database, username, pass).ToString()
            }
        };
        if (Directory.Exists(_backup.BackupConfig.DirRoot)) Directory.Delete(_backup.BackupConfig.DirRoot, true);

        Mock<IRestoreDownload> mock = new();
        mock.Setup(d => d.DownloadFileAsync(It.IsAny<RestoreWorkerConfig>(), It.IsAny<RestoreWorkerState>(), It.IsAny<CancellationToken>()))
            .Returns((RestoreWorkerConfig config, RestoreWorkerState state, CancellationToken c) =>
            {
                var src = _backup.GetFileBackup(state.DownloadedVersion, out var version);
                TestContext.WriteLine($"DownloadFileAsync: {state.DownloadedVersion} => {version}");
                if (src != null && version != null)
                {
                    var file = config.GetFilePathData(version);
                    File.Copy(src, file);
                    TestContext.WriteLine($"DownloadFileAsync File.Copy {src} => {file}");
                }
                return Task.FromResult(version);
            });

        _restore = new RestoreWorker
        {
            RestoreConfig = new RestoreWorkerConfig
            {
                SqlConnectString = SqlServerExecuterHelper.CreateConnectionString(server, database + "_copy", username, pass).ToString(),
                BackupAddress = "http://localhost:5000/",
                IdBackupWorker = _backup.Config.Id,
            },
            RestoreDownload = mock.Object
        };
        if (Directory.Exists(_restore.RestoreConfig.DirRoot)) Directory.Delete(_restore.RestoreConfig.DirRoot, true);
    }

    [Test]
    public async Task Backup()
    {
        FastQuery source() => SqlServerExecuterHelper.CreateConnection(_backup.Config.SqlConnectString!).CreateFastQuery();
        FastQuery destination() => SqlServerExecuterHelper.CreateConnection(_restore.Config.SqlConnectString!).CreateFastQuery();

        async Task CheckCount(FastQuery fast, int expected)
        {
            using var row = await fast.WithQuery("select Count(1) from Products;")
                 .ExecuteScalarAsync<int>();
            Assert.That(row.Result, Is.EqualTo(expected));
        }

        async Task InsertRow(FastQuery fast)
        {
            using var a = await fast
                 .WithQuery("insert into Products (CreateTime) values(GETDATE())")
                 .ExecuteNonQueryAsync();
        }

        await source().SetDatabaseReadOnly(false);
        _ = await source().WithQuery("DELETE Products").ExecuteNonQueryAsync();

        //FULL
        await InsertRow(source());
        await CheckCount(source(), 1);
        bool ok = await _backup.BackupFullAsync();
        Assert.That(ok, Is.True);

        //LOG
        await InsertRow(source());
        await CheckCount(source(), 2);
        ok = await _backup.BackupLogAsync(); Assert.That(ok, Is.True);

        //LOG
        await InsertRow(source());
        await CheckCount(source(), 3);
        ok = await _backup.BackupLogAsync(); Assert.That(ok, Is.True);

        //Restore
        await _restore.DownloadNewBackupAsync(_tokenSource.Token);
        await _restore.RestoreAsync(_tokenSource.Token);
        await CheckCount(destination(), 3);

        //LOG
        await InsertRow(source());
        await CheckCount(source(), 4);
        ok = await _backup.BackupLogAsync(); Assert.That(ok, Is.True);

        //Restore
        await _restore.DownloadNewBackupAsync(_tokenSource.Token);
        await _restore.RestoreAsync(_tokenSource.Token);
        await CheckCount(destination(), 4);

        //Restore
        await _restore.DownloadNewBackupAsync(_tokenSource.Token);
        await _restore.RestoreAsync(_tokenSource.Token);
        await CheckCount(destination(), 4);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _tokenSource.Cancel();
        //await _container.DisposeAsync();
        Trace.Flush();
    }

}
