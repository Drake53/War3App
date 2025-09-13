using System;
using System.Linq.Expressions;

using Eto.Drawing;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.EtoForms.Helpers;

namespace War3App.MapAdapter.EtoForms.Models
{
    public class FileListItem
    {
        public FileListItem(MapFile mapFile)
        {
            FileName = mapFile.OriginalFileName ?? MiscStrings.UnknownFileName;
            FileType = mapFile.Adapter?.MapFileDescription ?? string.Empty;
            Archive = mapFile.ArchiveName;
            MapFile = mapFile;
        }

        public string FileName { get; set; }

        public string FileType { get; set; }

        public string Archive { get; set; }

        public MapFile MapFile { get; }

        public static Expression<Func<FileListItem, string>> GetStatusTextExpression()
        {
            return item => item.MapFile.Status.ToString();
        }

        public static Expression<Func<FileListItem, Image>> GetStatusImageExpression()
        {
            return item => Icons.ForMapFile(item.MapFile);
        }

        public override string ToString() => FileName;
    }
}