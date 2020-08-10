using System.Windows.Forms;

namespace War3App.MapAdapter.WinForms.Extensions
{
    public static class ListViewExtensions
    {
        public static bool TryGetSelectedItem(this ListView listView, out ListViewItem item)
        {
            if (listView.SelectedItems.Count == 1)
            {
                item = listView.SelectedItems[0];
                return true;
            }

            item = null;
            return false;
        }

        public static bool TryGetSelectedItemTag(this ListView listView, out ItemTag tag)
        {
            if (listView.SelectedItems.Count == 1)
            {
                tag = listView.SelectedItems[0].GetTag();
                return true;
            }

            tag = null;
            return false;
        }
    }
}