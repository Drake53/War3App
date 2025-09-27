using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace War3App.MapAdapter.WinForms.Extensions
{
    public static class ListViewExtensions
    {
        public static bool TryGetSelectedItem(this ListView listView, [NotNullWhen(true)] out ListViewItem? item)
        {
            if (listView.SelectedItems.Count == 1)
            {
                item = listView.SelectedItems[0];
                return true;
            }

            item = null;
            return false;
        }

        public static bool TryGetSelectedMapFile(this ListView listView, [NotNullWhen(true)] out MapFile? mapFile)
        {
            if (listView.SelectedItems.Count == 1)
            {
                mapFile = listView.SelectedItems[0].GetMapFile();
                return true;
            }

            mapFile = null;
            return false;
        }

        public static MapFile[] GetSelectedMapFiles(this ListView listView)
        {
            var mapFiles = new MapFile[listView.SelectedItems.Count];
            for (var i = 0; i < listView.SelectedItems.Count; i++)
            {
                mapFiles[i] = listView.SelectedItems[i].GetMapFile();
            }

            return mapFiles;
        }
    }
}