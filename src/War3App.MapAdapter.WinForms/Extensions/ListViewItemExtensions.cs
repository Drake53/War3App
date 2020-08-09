using System.Linq;
using System.Windows.Forms;

namespace War3App.MapAdapter.WinForms.Extensions
{
    public static class ListViewItemExtensions
    {
        internal const int StatusColumnIndex = 0;
        internal const int FileNameColumnIndex = 1;
        internal const int ArchiveNameColumnIndex = 2;

        public static ListViewItem Create(ItemTag tag)
        {
            var item = new ListViewItem(new[]
            {
                string.Empty,
                tag.FileName ?? "<unknown filename>",
                tag.ArchiveName,
            });

            item.Tag = tag;
            tag.ListViewItem = item;

            item.Update();
            return item;
        }

        public static ItemTag GetTag(this ListViewItem item)
        {
            return (ItemTag)item.Tag;
        }

        public static bool IsUnknown(this ListViewItem item)
        {
            return item.GetTag().Status == MapFileStatus.Unknown;
        }

        public static string GetFileName(this ListViewItem item)
        {
            return item.GetTag().FileName;
        }

        public static void Update(this ListViewItem item)
        {
            var tag = item.GetTag();
            tag.Status = tag.Children != null
                ? tag.Children.Max(child => child.Status)
                : tag.Status;

            item.SubItems[StatusColumnIndex].Text = tag.Status.ToString();
            item.ImageIndex = (int)tag.Status;
        }

        public static void Update(this ListViewItem item, AdaptResult adaptResult)
        {
            var tag = item.GetTag();
            tag.AdaptResult = adaptResult;

            item.SubItems[StatusColumnIndex].Text = tag.Status.ToString();
            item.ImageIndex = (int)tag.Status;
        }
    }
}