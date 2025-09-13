using System.Linq;

using War3App.MapAdapter.Constants;

namespace War3App.MapAdapter.WinForms.Constants
{
    public static class FilterStrings
    {
        public static readonly string AllFiles = BuildFilterString(FileType.Any, FileExtension.AnyFile);
        public static readonly string ArchiveFile = BuildFilterString(FileType.Archive, FileExtension.Map, FileExtension.MapEx, FileExtension.Campaign);
        public static readonly string CampaignFile = BuildFilterString(FileType.Campaign, FileExtension.Campaign);
        public static readonly string MapFile = BuildFilterString(FileType.Map, FileExtension.Map, FileExtension.MapEx);
        public static readonly string ZipArchive = BuildFilterString(FileType.Zip, FileExtension.Zip);
        public static readonly string ZipFileOrAllFiles = Combine(ZipArchive, AllFiles);

        public static string Combine(params string[] filterStrings)
        {
            return string.Join('|', filterStrings);
        }

        private static string BuildFilterString(string name, params string[] extensions)
        {
            return $"{name}|{string.Join(';', extensions.Select(extension => $"*{extension}"))}";
        }
    }
}