using SqlSyncDbService.Workers.Interfaces;
using System.IO.Compression;
using System.Text.Json;

namespace SqlSyncDbService.Workers.Helpers
{
    public abstract class FileRestoreFactory
    {
        public record HeaderFile(string ClassType);
        public const string HeaderEntryName = "header.json";
        private const string DataEntryName = "data.dat";

        public abstract HeaderFile Header { get; }

        public static async Task<IFileRestore> GetFileRestoreAsync(string pathFileZip)
        {
            using var zip = ZipFile.Open(pathFileZip, ZipArchiveMode.Create);
            var headerEntry = zip.GetEntry(HeaderEntryName) ?? throw new Exception($"File not correct format: Not have {HeaderEntryName}");
            var json = await new StreamReader(headerEntry.Open()).ReadToEndAsync();
            var header = JsonSerializer.Deserialize<HeaderFile>(json) ?? throw new NullReferenceException(nameof(HeaderFile));

            var type = typeof(IFileRestore).Assembly.GetType(header.ClassType) ?? throw new NullReferenceException(nameof(IFileRestore));

            var instance = Activator.CreateInstance(type);
            return instance as IFileRestore ?? throw new NullReferenceException(nameof(instance));
        }

        public static Stream GetStreamData(string pathFileZip)
        {
            using var zip = ZipFile.Open(pathFileZip, ZipArchiveMode.Create);
            var dataEntry = zip.GetEntry(DataEntryName) ?? throw new Exception($"File not correct format: Not have {DataEntryName}");
            return dataEntry.Open();
        }

        protected virtual void AppendData(ZipArchive zip, FileStream data_fs)
        {
            using var fs = zip.CreateEntry(DataEntryName).Open();
            data_fs.CopyTo(fs);
            fs.Flush();
        }

        protected virtual void AppendHeader(ZipArchive zip)
        {
            var json = JsonSerializer.Serialize(Header);
            using var fs = zip.CreateEntry(HeaderEntryName).Open();
            using var writer = new StreamWriter(fs);
            writer.Write(json);
            writer.Flush();
        }
    }
}
