using System.Globalization;
using SimpleStore.Admin.Services.v1;

namespace SimpleStore.Admin.Extensions;

public static class StorageInfoExtensions
{
    public static string FreeGbFormatted(this StorageInfoDto storageStats) => storageStats.FreeGB.ToString("F1", CultureInfo.InvariantCulture);
    public static string SizeGbFormatted(this StorageInfoDto storageStats) => storageStats.SizeGB.ToString("F1", CultureInfo.InvariantCulture);
}