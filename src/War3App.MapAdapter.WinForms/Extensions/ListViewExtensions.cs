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

        public static bool TryGetSelectedItemTag(this ListView listView, [NotNullWhen(true)] out ItemTag? tag)
        {
            if (listView.SelectedItems.Count == 1)
            {
                tag = listView.SelectedItems[0].GetTag();
                return true;
            }

            tag = null;
            return false;
        }

        public static ItemTag[] GetSelectedItemTags(this ListView listView)
        {
            var tags = new ItemTag[listView.SelectedItems.Count];
            for (var i = 0; i < listView.SelectedItems.Count; i++)
            {
                tags[i] = listView.SelectedItems[i].GetTag();
            }

            return tags;
        }
    }
}