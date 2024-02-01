using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SqlSyncDbServiceLib.Helpers
{
    public abstract class FileRestoreFactory
    {
        public const string HeaderEntryName = "header.json";
        private const string DataEntryName = "data.dat";

        public abstract HeaderFile Header { get; }

        public static Dictionary<string, Type> GetIFileRestoreClass()
        {
            var type = typeof(IFileRestore);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .ToList();
            var result = new Dictionary<string, Type>();
            foreach (var typeClass in types)
            {
                result.Add(typeClass.FullName, typeClass);
            }
            return result;
        }

        static Lazy<Dictionary<string, Type>> ClassTypeCached = new Lazy<Dictionary<string, Type>>(GetIFileRestoreClass);

        public static async Task<IFileRestore> GetFileRestoreAsync(string pathFileZip)
        {
            using (var zip = ZipFile.Open(pathFileZip, ZipArchiveMode.Read))
            {
                var headerEntry = zip.GetEntry(HeaderEntryName) ?? throw new Exception($"File not correct format: Not have {HeaderEntryName}");
                var stream = headerEntry.Open();
                var header = HeaderFile.FromStream(stream) ?? throw new NullReferenceException(nameof(HeaderFile));

                if (ClassTypeCached.Value.TryGetValue(header.ClassType, out var type))
                {
                    var instance = Activator.CreateInstance(type);
                    return instance as IFileRestore ?? throw new NullReferenceException(nameof(instance));
                }
                throw new NullReferenceException(nameof(IFileRestore));
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
