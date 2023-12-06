using Azure;
using FastQueryLib;
using Microsoft.Data.SqlClient;
using SqlSyncLib.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Threading.Channels;
using System.Xml.Linq;

namespace SqlSyncLib.LogicRestore
{
    public interface IPrimaryServer : IServer
    {
        Task<IHeaderBackupItem?> NotifyBackupItemAsync(IHeaderBackupItem header);
        Task<IHeaderBackupItem> SendBackupItemAsync(IHeaderBackupItem itemSync);
    }

    public interface IServer : IDisposable
    {
        Task RunAsync(CancellationToken cancellationToken);
    }

    public interface IBackupDatabase
    {
        Task<IBackupItem?> CreateBackupAsync(InfoBackupSetting InfoBackupSetting);
        Task<bool> RestoreBackupAsync(InfoBackupSetting InfoBackupSetting, IBackupItem backupItem);
        JobScheduleSetting ScheduleSetting { get; }
    }

    public record InfoBackupSetting(string sqlConnectString, string PathFolder) { }

    public record PrimaryServerSetting(InfoBackupSetting InfoBackupSetting, InfoBackupServer InfoBackupServer)
    {
    }

    public record JobScheduleSetting(TimeSpan delayTime)
    {
        public DateTime? LastReset { get; set; }
    }

    public interface ILogger
    {
        void Log(object? message);
    }

    public abstract class ServerBase : IServer
    {
        public ServerBase(ILogger log,
                          IWorkflowBackup workflowBackup)
        {
            _workflowBackup = workflowBackup;
            _logger = log;
        }

        protected CancellationTokenSource? _mainToken { get; set; }
        protected readonly ILogger _logger;
        protected readonly IWorkflowBackup _workflowBackup;

        public virtual void Dispose()
        {
            _mainToken?.Dispose();
        }

        public abstract Task RunAsync(CancellationToken cancellationToken);

        protected virtual async Task<HttpResponseMessage> _withHttpClient(Func<HttpClient, Task<HttpResponseMessage>> useHttpClient)
        {
            using var httpClient = new HttpClient();
            var response = await useHttpClient.Invoke(httpClient);
            _logger.Log($"{response.RequestMessage?.Method}-{response.StatusCode}: {response.RequestMessage?.RequestUri}");
            return response;
        }

        protected virtual async Task<T?> _tryMethod<T>(Func<Task<T>> value)
        {
            try
            {
                return await value.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _logger.Log(ex);
            }
            return default;
        }

    }

    public interface IWorkflowBackup
    {
        List<IBackupDatabase> BackupDatabases { get; }

        List<IBackupDatabase> GonnaBackup(DateTime triggerTime);
    }

    public class WorkflowBackup : IWorkflowBackup
    {
        public List<IBackupDatabase> BackupDatabases { get; set; } = new List<IBackupDatabase>()
        {
            new FullBackupDatabase(),
            new LogBackupDatabase(),
        };

        public List<IBackupDatabase> GonnaBackup(DateTime triggerTime)
        {
            var backups = BackupDatabases.Where(q =>
            {
                var diff = triggerTime - q.ScheduleSetting.LastReset;
                return diff is null || diff > q.ScheduleSetting.delayTime;
            }).ToList();
            return backups;
        }
    }

    public class LogBackupDatabase : BackupDatabaseBase, IBackupDatabase
    {
        public JobScheduleSetting ScheduleSetting { get; } = new JobScheduleSetting(TimeSpan.FromMinutes(1));

        protected override string GetQueryBackup(string dbName, FileHeaderBackupItem header)
        {
            var pathFile = header.GetPathFile();
            var query = $" BACKUP LOG [{dbName}] TO DISK='{pathFile}' WITH FORMAT; ";
            return query;
        }

        protected override string GetQueryRestore(string dbName, FileBackupItem fileBackupItem)
        {
            var standby = $"{dbName}.standby";
            var pathFile = fileBackupItem.HeaderFile.GetPathFile();
            var query = $" RESTORE LOG [{dbName}] FROM DISK='{pathFile}' WITH REPLACE, STANDBY='{standby}'; ";
            return query;
        }
    }

    public abstract class BackupDatabaseBase
    {
        public virtual async Task<bool> ApplyAsync(InfoBackupSetting InfoBackupSetting, string query)
        {
            using var conn = new SqlConnection(InfoBackupSetting.sqlConnectString);
            var dbName = conn.Database;
            using var master = conn.NewOpenConnectToDatabase("master");
            using var restoreJob = await master.CreateFastQuery().WithQuery(query).ExecuteNumberOfRowsAsync();
            return await master.CheckDatabaseExistsAsync(dbName);
        }

        public virtual async Task<IBackupItem?> CreateBackupAsync(InfoBackupSetting InfoBackupSetting)
        {
            var dbName = new SqlConnectionStringBuilder(InfoBackupSetting.sqlConnectString).InitialCatalog;
            var version = VersionFactory.Instance.GetNewVersion();
            var header = new FileHeaderBackupItem(version, InfoBackupSetting.PathFolder, $"{dbName}.{{0}}.full.bak");
            var query = GetQueryBackup(dbName, header);
            var backupSuccess = await ApplyAsync(InfoBackupSetting, query);
            if (!backupSuccess) return default;
            return new FileBackupItem(header);
        }

        protected abstract string GetQueryBackup(string dbName, FileHeaderBackupItem header);

        public virtual async Task<bool> RestoreBackupAsync(InfoBackupSetting InfoBackupSetting, IBackupItem backupItem)
        {
            if (backupItem is FileBackupItem fileBackupItem)
            {
                var dbName = new SqlConnectionStringBuilder(InfoBackupSetting.sqlConnectString).InitialCatalog;
                var query = GetQueryRestore(dbName, fileBackupItem);
                var backupSuccess = await ApplyAsync(InfoBackupSetting, query);
                return backupSuccess;
            }
            return false;
        }

        protected abstract string GetQueryRestore(string dbName, FileBackupItem fileBackupItem);
    }

    public record FileBackupItem(FileHeaderBackupItem HeaderFile) : IBackupItem
    {
        public IHeaderBackupItem Header => HeaderFile;

        public virtual Stream GetStreamData()
        {
            return new FileStream(HeaderFile.GetPathFile(), FileMode.Open);
        }
    }

    public record FileHeaderBackupItem(string Version, string pathFolder, string templateFileName) : IHeaderBackupItem
    {
        public string GetPathFile(bool ensureDir = true)
        {
            var filename = string.Format(templateFileName, Version);
            if (ensureDir && !Directory.Exists(pathFolder)) Directory.CreateDirectory(pathFolder);
            return Path.Combine(pathFolder, filename);
        }

        public Task<IBackupItem> GetBackupItemAsync()
        {
            IBackupItem backupItem = new FileBackupItem(this);
            return Task.FromResult(backupItem);
        }
    }

    public class VersionFactory
    {
        public static readonly VersionFactory Instance = new VersionFactory();

        public string GetNewVersion()
        {
            return DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss");
        }
    }

    public class FullBackupDatabase : BackupDatabaseBase, IBackupDatabase
    {
        public JobScheduleSetting ScheduleSetting { get; set; } = new JobScheduleSetting(TimeSpan.FromDays(1));

        protected override string GetQueryBackup(string dbName, FileHeaderBackupItem header)
        {
            var pathFile = header.GetPathFile();
            var query = $" BACKUP DATABASE [{dbName}] TO DISK='{pathFile}' WITH FORMAT; ";
            return query;
        }

        protected override string GetQueryRestore(string dbName, FileBackupItem fileBackupItem)
        {
            var standby = $"{dbName}.standby";
            var pathFile = fileBackupItem.HeaderFile.GetPathFile();
            var query = $" RESTORE DATABASE [{dbName}] FROM DISK='{pathFile}' WITH REPLACE, STANDBY='{standby}'; ";
            return query;
        }
    }

    public abstract class PrimaryServer : ServerBase, IPrimaryServer
    {
        public PrimaryServer(PrimaryServerSetting primaryServerSetting,
                             ILogger log,
                             IWorkflowBackup workflowBackup)
            : base(log, workflowBackup)
        {
            _primaryServerSetting = primaryServerSetting;
        }
        private readonly PrimaryServerSetting _primaryServerSetting;
        private Channel<IHeaderBackupItem?> _channelNotify;
        private Channel<IHeaderBackupItem?> _channelSendBackup;

        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            this._channelNotify = Channel.CreateUnbounded<IHeaderBackupItem?>();
            this._channelSendBackup = Channel.CreateUnbounded<IHeaderBackupItem?>();

            this._mainToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _mainToken.Token.Register(() =>
            {
                _channelNotify.Writer.Complete();
                _channelSendBackup.Writer.Complete();
            });

            _ = Task.Run(async () => await NotifyAsync(_channelNotify.Reader), _mainToken.Token);
            _ = Task.Run(async () => await SendBackupAsync(_channelSendBackup.Reader), _mainToken.Token);

            while (_mainToken.IsCancellationRequested == false)
            {
                try
                {
                    var now = DateTime.Now;
                    var backups = _workflowBackup.GonnaBackup(now);
                    var task_headerBackups = backups.Select(q => q.CreateBackupAsync(_primaryServerSetting.InfoBackupSetting)).ToList();
                    var headerBackups = await Task.WhenAll(task_headerBackups);
                    var headerBackupSuccess = headerBackups.Where(q => q is not null).ToList();

                    foreach (var item in headerBackupSuccess)
                    {
                        _channelNotify.Writer.TryWrite(item?.Header);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    _logger.Log(ex);
                }

                //delay
                _logger.Log($"Delay: 5s");
                await Task.Delay(5000, cancellationToken);
            }
        }

        private async Task SendBackupAsync(ChannelReader<IHeaderBackupItem?> reader)
        {
            while (_mainToken?.IsCancellationRequested == false)
            {
                var notiHeader = await reader.ReadAsync();
                if (notiHeader == null) continue;

                //resolve rep noti
                var sender = await SendBackupItemAsync(notiHeader);
                _logger.Log($"{nameof(SendBackupItemAsync)}: {notiHeader} => {sender}");
            }
        }

        private async Task NotifyAsync(ChannelReader<IHeaderBackupItem?> reader)
        {
            while (_mainToken?.IsCancellationRequested == false)
            {
                var headerBackup = await reader.ReadAsync();
                if (headerBackup == null) continue;

                var notiHeader = await NotifyBackupItemAsync(headerBackup);
                _logger.Log($"{nameof(NotifyBackupItemAsync)}: {headerBackup} => {notiHeader}");
                _channelSendBackup.Writer.TryWrite(notiHeader);
            }
        }

        public virtual Task<IHeaderBackupItem?> NotifyBackupItemAsync(IHeaderBackupItem header)
        {
            return _tryMethod(async () =>
            {
                var response = await _withHttpClient(async client =>
                {
                    return await client.PostAsJsonAsync(nameof(NotifyBackupItemAsync), header);
                });
                if (!response.IsSuccessStatusCode) return default;
                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(json);
                var request = JsonSerializer.Deserialize<IHeaderBackupItem>(json);
                return request;
            });
        }

        public virtual Task<bool> SendBackupItemAsync(IHeaderBackupItem itemSync)
        {
            return _tryMethod(async () =>
            {
                var backup = await itemSync.GetBackupItemAsync();
                if (backup == null) return false;

                var response = await _withHttpClient(async client =>
                {
                    var headerJson = JsonSerializer.Serialize(backup.Header);
                    var url = nameof(SendBackupItemAsync);
                    using var request = new HttpRequestMessage(HttpMethod.Post, url);
                    using var stream = backup.GetStreamData();
                    using var content = new MultipartFormDataContent
                    {
                        { new StreamContent(stream), "data" },
                        { new StringContent(headerJson), "headerJson" }
                    };
                    request.Content = content;
                    return await client.SendAsync(request);
                });

                if (!response.IsSuccessStatusCode) return false;

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(json);
                return true;
            });
        }
    }

    public interface IHeaderBackupItem
    {
        Task<IBackupItem> GetBackupItemAsync();
    }

    public interface IBackupItem
    {
        IHeaderBackupItem Header { get; }
        Stream GetStreamData();
    }

    public record InfoBackupServer(string BaseAddress);

    public interface IBackupServer : IServer
    {
        InfoBackupServer InfoBackupServer { get; }
        Task<IHeaderBackupItem> ReplyNotifyBackupItem(IHeaderBackupItem header);
        Task<IHeaderBackupItem> ReceiverBackupItemAsync(IBackupItem itemSync);
    }

    public record BackupServerSetting(InfoBackupSetting InfoBackupSetting, InfoBackupServer InfoBackupServer) { };

    public abstract class BackupServer : ServerBase, IBackupServer
    {
        protected BackupServer(ILogger log,
                               IWorkflowBackup workflowBackup,
                               BackupServerSetting setting) : base(log, workflowBackup)
        {
            Setting = setting;
        }

        public virtual BackupServerSetting Setting { get; }

        public virtual InfoBackupServer InfoBackupServer => Setting.InfoBackupServer;

        public virtual async Task<bool> ReceiverBackupItemAsync(IBackupItem itemSync)
        {
            var backups = _workflowBackup.BackupDatabases;
            var success = false;
            foreach (var backup in backups)
            {
                if (_mainToken?.IsCancellationRequested ?? true) break;
                if (await backup.RestoreBackupAsync(Setting.InfoBackupSetting, itemSync))
                {
                    success = true;
                }
            }
            return success;
        }

        public virtual Task<IHeaderBackupItem> ReplyNotifyBackupItem(IHeaderBackupItem header)
        {

        }

        public virtual override Task RunAsync(CancellationToken cancellationToken)
        {

        }
    }
}
