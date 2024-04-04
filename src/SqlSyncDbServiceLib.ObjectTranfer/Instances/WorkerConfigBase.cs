using System.Data.SqlClient;
using SqlSyncDbServiceLib.Helpers;
using SqlSyncDbServiceLib.ObjectTranfer.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SqlSyncDbServiceLib
{
    public abstract class WorkerConfigBase : IWorkerConfig
    {
        public virtual string Id { get; set; } = VersionFactory.Instance.GetNewVersion();
        public virtual TimeSpan DelayTime { get; set; } = TimeSpan.FromSeconds(8);
        public virtual string DirRoot { get; set; } = "./data/DbName/";
        public virtual string DirData { get; set; } = "./data/DbName/base";
        public virtual EnumWorkerMode workerMode { get; set; } = EnumWorkerMode.Auto;
        public virtual bool IsAuto => workerMode == EnumWorkerMode.Auto;

        public virtual string DbName { get; private set; }

        private string sqlConnectString;
        public virtual string SqlConnectString
        {
            get => sqlConnectString;
            set
            {
                OnUpdateSqlConnectionString(value, sqlConnectString);
                sqlConnectString = value;
            }
        }

        public virtual void OnUpdateSqlConnectionString(string newValue, string oldValue)
        {
            DbName = new SqlConnectionStringBuilder(newValue).InitialCatalog;
        }

        public virtual string GetFilePathData(string version, bool ensureFolder = true)
        {
            if (string.IsNullOrWhiteSpace(DirData))
            {
                throw new NullReferenceException(nameof(DirData));
            }
            var path = Path.Combine(DirData, $"{version}.syncdb");
            if (ensureFolder && !Directory.Exists(DirData)) Directory.CreateDirectory(DirData);
            return Path.GetFullPath(path);
        }


        #region SAVE/LOAD/NextVersion STATES

        public virtual bool SaveState(WorkerStateVersionBase state)
            => state.SaveState(WorkerStateVersionBase.GetFilePathState(DirData, state.CurrentVersion));

        public virtual T GetStateByVersion<T>(string version) where T : WorkerStateVersionBase
        {
            var file = WorkerStateVersionBase.GetFilePathState(DirData, version);
            return WorkerStateVersionBase.GetStateByVersion<T>(file);
        }

        public virtual string GetNextVersion<T>(string currentVersion, T currentState) where T : WorkerStateVersionBase
            => currentState.GetNextVersion<T>(DirData, currentVersion);

        #endregion

        public virtual async Task ValidateSettingAsync(CancellationToken cancellationToken)
        {
            //check IsNullOrWhiteSpace
            var notAllowEmptys = new[] {
                SqlConnectString,
                Id,
                DirRoot,
                DirData,
            };
            ValidateSettingIsNullOrWhiteSpace("SqlConnectString, Id, DirRoot, DirData", notAllowEmptys);

            //check SqlConnectString
            try
            {
                using (var conn = new SqlConnection(SqlConnectString))
                {
                    await conn.OpenAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DbName} => {ex.Message}");
            }
        }

        public virtual void ValidateSettingIsNullOrWhiteSpace(string name, params string[] notAllowEmptys)
        {
            if (notAllowEmptys.Any(string.IsNullOrWhiteSpace))
            {
                throw new Exception($"Please make sure setting have value: {name}");
            }
        }
    }
}
