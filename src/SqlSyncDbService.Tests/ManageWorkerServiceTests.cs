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

    [OneTimeSetUp]
    public void Setup()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        _tokenSource = new CancellationTokenSource();

        _backup = new BackupWorker
        {
            BackupConfig = new BackupWorkerConfig
            {
                //SqlConnectString = FastQueryLib.SqlServerExecuterHelper.CreateConnectionString(".\\SQLEXPRESS", "A").ToString()
                SqlConnectString = FastQueryLib.SqlServerExecuterHelper.CreateConnectionString(".", "A", "dev", "1").ToString()
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
                //SqlConnectString = FastQueryLib.SqlServerExecuterHelper.CreateConnectionString(".\\SQLEXPRESS", "A_copy").ToString(),
                SqlConnectString = FastQueryLib.SqlServerExecuterHelper.CreateConnectionString(".", "A_copy", "dev", "1").ToString(),
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
        var ok = false;

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
        //delete data
        using var _ = await source().WithQuery("DELETE Products").ExecuteNonQueryAsync();

        //FULL
        await InsertRow(source());
        await CheckCount(source(), 1);
        ok = await _backup.BackupFullAsync(); Assert.That(ok, Is.True);

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
        Trace.Flush();
    }

}
