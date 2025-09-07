using System.Windows.Forms;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.WinForms.Controls;

namespace War3App.MapAdapter.WinForms.Helpers
{
    public static class ListViewItemFactory
    {
        public static ListViewItem Create(MapFile mapFile, FileListView fileList)
        {
            var item = new ListViewItem(new[]
            {
                string.Empty,
                mapFile.OriginalFileName ?? MiscStrings.UnknownFileName,
                mapFile.Adapter?.MapFileDescription ?? string.Empty,
                mapFile.ArchiveName,
            });

            item.UseItemStyleForSubItems = false;
            item.Tag = mapFile;
            fileList.AddMapping(mapFile, item);

            return item;
        }
    }
}