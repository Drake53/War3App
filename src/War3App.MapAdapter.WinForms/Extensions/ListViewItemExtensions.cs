using System;
using System.Linq;
using System.Windows.Forms;

namespace War3App.MapAdapter.WinForms.Extensions
{
    public static class ListViewItemExtensions
    {
        internal const int StatusColumnIndex = 0;
        internal const int FileNameColumnIndex = 1;
        internal const int FileTypeColumnIndex = 2;
        internal const int ArchiveNameColumnIndex = 3;

        public static ListViewItem Create(ItemTag tag)
        {
            var item = new ListViewItem(new[]
            {
                string.Empty,
                tag.FileName ?? "<unknown filename>",
                tag.Adapter?.MapFileDescription ?? string.Empty,
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

        public static int CompareTo(this ListViewItem item, ListViewItem other, int column)
        {
            return column switch
            {
                StatusColumnIndex => 0 - item.GetTag().Status.CompareTo(other.GetTag().Status),

                _ => string.IsNullOrWhiteSpace(item.SubItems[column].Text) == string.IsNullOrWhiteSpace(other.SubItems[column].Text)
                    ? string.Compare(item.SubItems[column].Text, other.SubItems[column].Text, StringComparison.InvariantCulture)
                    : string.IsNullOrWhiteSpace(item.SubItems[column].Text) ? 1 : -1,
            };
        }
    }
}