using SqlSyncDbServiceLib.ObjectTranfer.Instances;
using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.Helpers
{
    public abstract class FileRestoreFactory
    {
        public class HeaderFile
        {
            public string ClassType { get; set; }
            public BackupWorkerState WorkerState { get; set; }

            public HeaderFile(string classType, BackupWorkerState workerState)
            {
                ClassType = classType;
                WorkerState = workerState;
            }

            public void WriteToStream(Stream outStream)
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(outStream, this);
                outStream.Flush();
            }

            public static HeaderFile FromStream(Stream stream)
            {
                var binaryFormatter = new BinaryFormatter();
                var obj = binaryFormatter.Deserialize(stream);
                return obj as HeaderFile;
            }
        }

        public const string HeaderEntryName = "header.json";
        private const string DataEntryName = "data.dat";

        public abstract HeaderFile Header { get; }

        public static async Task<IFileRestore> GetFileRestoreAsync(string pathFileZip)
        {
            using (var zip = ZipFile.Open(pathFileZip, ZipArchiveMode.Read))
            {
                var headerEntry = zip.GetEntry(HeaderEntryName) ?? throw new Exception($"File not correct format: Not have {HeaderEntryName}");
                var stream = headerEntry.Open();
                var header = HeaderFile.FromStream(stream) ?? throw new NullReferenceException(nameof(HeaderFile));

                var type = typeof(IFileRestore).Assembly.GetType(header.ClassType) ?? throw new NullReferenceException(nameof(IFileRestore));

                var instance = Activator.CreateInstance(type);
                return instance as IFileRestore ?? throw new NullReferenceException(nameof(instance));
            }
        }

        public static void SaveStreamData(string pathFileZip, string file)
        {
            using (var zip = ZipFile.Open(pathFileZip, ZipArchiveMode.Read))
            {
                var dataEntry = zip.GetEntry(DataEntryName) ?? throw new Exception($"File not correct format: Not have {DataEntryName}");
                dataEntry.ExtractToFile(file, true);
            }
        }

        public static HeaderFile GetHeaderFile(string pathFileZip)
        {
            using (var zip = ZipFile.Open(pathFileZip, ZipArchiveMode.Read))
            {
                var entry = zip.GetEntry(HeaderEntryName);
                if (entry == null) return default;
                return HeaderFile.FromStream(entry.Open());
            }
        }

        protected virtual void AppendData(ZipArchive zip, FileStream data_fs)
        {
            using (var fs = zip.CreateEntry(DataEntryName).Open())
            {

                data_fs.CopyTo(fs);
                fs.Flush();
            }
        }

        protected virtual void AppendHeader(ZipArchive zip)
        {
            using (var fs = zip.CreateEntry(HeaderEntryName).Open())
            {
                Header.WriteToStream(fs);
            }
        }

    }
}
