using FastQueryLib;
using System.Data.SqlClient;
using Moq;
using SqlSyncDbServiceLib.BackupWorkers;
using SqlSyncDbServiceLib.ObjectTranfer.Instances;
using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using SqlSyncDbServiceLib.RestoreWorkers;
using System.Diagnostics;

namespace SqlSyncDbService.Tests;

public class ManageWorkerServiceTests
{
    private CancellationTokenSource _tokenSource;
    private BackupWorker _backup;
    private RestoreWorker _restore;

#if DEBUG0
    readonly string SERVER = ".\\SQLEXPRESS";
#else
    readonly string SERVER = ".";
#endif
    readonly string DATABASE = "dbX";

    [OneTimeSetUp]
    public async Task Setup()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        //create db
        var masterConnectionString = SqlServerExecuterHelper.CreateConnectionString(SERVER, "master").ToString();
        using var fastQuery = await new SqlConnection(masterConnectionString).CreateFastQuery()
             .WithQuery("IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'dbX') BEGIN CREATE DATABASE dbX; END")
             .ExecuteNonQueryAsync();

        using var fastQuery2 = await new SqlConnection(SqlServerExecuterHelper.CreateConnectionString(SERVER, "dbX").ToString()).CreateFastQuery()
             .WithQuery(@"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
BEGIN
    CREATE TABLE Products (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        CreateTime DATETIME
    );
END")
             .ExecuteNonQueryAsync();

        _tokenSource = new CancellationTokenSource();

        Mock<ISqlSyncDbServiceLibLogger> loggerMock = new();
        loggerMock.Setup(d => d.Log(It.IsAny<object>()))
            .Callback<object>(o => Debug.WriteLine(o));

        _backup = new BackupWorker(loggerMock.Object)
        {
            BackupConfig = new BackupWorkerConfig
            {
                SqlConnectString = SqlServerExecuterHelper.CreateConnectionString(SERVER, DATABASE).ToString()
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
        
        _restore = new RestoreWorker(loggerMock.Object)
        {
            RestoreConfig = new RestoreWorkerConfig
            {
                SqlConnectString = SqlServerExecuterHelper.CreateConnectionString(SERVER, $"{DATABASE}_copy").ToString(),
                BackupAddress = "http://localhost:5000/",
                IdBackupWorker = _backup.Config.Id,
            },
            RestoreDownload = mock.Object
        };
        if (Directory.Exists(_restore.RestoreConfig.DirRoot))
        {
            try
            {
                Directory.Delete(_restore.RestoreConfig.DirRoot, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
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
        {
            using var a1 = await source().SetDatabaseReadOnly(false);
        }
        {
            using var a2 = await source().WithQuery("DELETE Products").ExecuteNonQueryAsync();
        }

        var row = 0;

        for (int i = 0; i < 3; i++)
        {
            //FULL
            await InsertRow(source());
            await CheckCount(source(), ++row);
            bool ok = await _backup.BackupFullAsync();
            Assert.That(ok, Is.True);

            //LOG
            await InsertRow(source());
            await CheckCount(source(), ++row);
            ok = await _backup.BackupLogAsync(); Assert.That(ok, Is.True);

            //LOG
            await InsertRow(source());
            await CheckCount(source(), ++row);
            ok = await _backup.BackupLogAsync(); Assert.That(ok, Is.True);

            //Restore
            await _restore.DownloadNewBackupAsync(_tokenSource.Token);
            await _restore.RestoreAsync(_tokenSource.Token);
            await CheckCount(destination(), row);

            //LOG
            await InsertRow(source());
            await CheckCount(source(), ++row);
            ok = await _backup.BackupLogAsync(); Assert.That(ok, Is.True);

            //Restore
            await _restore.DownloadNewBackupAsync(_tokenSource.Token);
            await _restore.RestoreAsync(_tokenSource.Token);
            await CheckCount(destination(), row);

            //Restore
            await _restore.DownloadNewBackupAsync(_tokenSource.Token);
            await _restore.RestoreAsync(_tokenSource.Token);
            await CheckCount(destination(), row);
        }
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _tokenSource.Cancel();
        Trace.Flush();
    }

}