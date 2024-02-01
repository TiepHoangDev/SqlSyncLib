using Newtonsoft.Json;
using SqlSyncDbServiceLib.ObjectTranfer.Instances;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace SqlSyncDbServiceLib.Helpers
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
            using (var writer = new StreamWriter(outStream))
            {
                var json = JsonConvert.SerializeObject(this);
                writer.Write(json);
                writer.Flush();
            }
        }

        public static HeaderFile FromStream(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<HeaderFile>(json);
            }
        }
    }
}
