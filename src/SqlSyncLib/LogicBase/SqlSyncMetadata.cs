using System.ComponentModel.DataAnnotations;

namespace SqlSyncLib.LogicBase
{
    /// <summary>
    /// CurrentVersion, MinVersion. Info data of sync database.
    /// </summary>
    /// <param name="CurrentVersion"></param>
    /// <param name="MinVersion"></param>
    public record SqlSyncMetadata(string CurrentVersion, string MinVersion)
    {
    }

}
