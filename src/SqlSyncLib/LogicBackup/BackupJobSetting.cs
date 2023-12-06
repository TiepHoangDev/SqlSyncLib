using System.Xml.Linq;
using System;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
using SqlSyncLib.LogicBase;

namespace SqlSyncLib.LogicBackup
{
    public record BackupJobSetting(string connectString) : Fileable
    {
        public string PathFolderRoot { get; set; } = "./backup";
        public readonly string DbName = new SqlConnectionStringBuilder(connectString).InitialCatalog;

        public override string FilePath => CreatePathFile("BackupJobSetting.json", PathFolderRoot);
        public string? GetLatestBackupFullVersion()
        {
            var file = GetLatestBackupFull();
            var match = Regex.Match(file ?? "", @"\.full\.(?<version>.+)\.bak$");
            if (match.Success) return match.Groups["version"].Value;
            return default;
        }

        public string? GetLatestBackupFull() => Directory.GetFiles(Path.Combine(PathFolderRoot, "backupFull")).OrderByDescending(q => q).FirstOrDefault();


        public string CreatePathFileBackupFull(string version)
        {
            var filename = $"{DbName}.full.{version}.bak";
            return CreatePathFile(filename, PathFolderRoot, "backupFull");
        }

        public string CreatePathFileBackupLog(string version)
        {
            var filename = $"{DbName}.log.{version}.bak";
            return CreatePathFile(filename, PathFolderRoot, "backupLogs");
        }

    }
}
