using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.WinForms.Controls;

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
                tag.OriginalFileName ?? "<unknown filename>",
                tag.Adapter?.MapFileDescription ?? string.Empty,
                tag.ArchiveName,
            });

            item.UseItemStyleForSubItems = false;
            item.Tag = tag;
            tag.ListViewItem = item;

            return item;
        }

        public static ItemTag GetTag(this ListViewItem item)
        {
            return (ItemTag)item.Tag;
        }

        public static void Update(this ListViewItem item)
        {
            var tag = item.GetTag();
            if (tag.Children is not null)
            {
                tag.Status = tag.Children.Max(child => child.Status);
            }

            item.SubItems[StatusColumnIndex].Text = tag.Status.ToString();
            item.SetImageIndex(tag, null);
        }

        public static void Update(this ListViewItem item, AdaptResult? adaptResult)
        {
            var tag = item.GetTag();
            tag.AdaptResult = adaptResult;

            item.SubItems[StatusColumnIndex].Text = tag.Status.ToString();
            item.SubItems[FileNameColumnIndex].ForeColor = adaptResult?.AdaptedFileStream is not null ? Color.Violet : Color.Black;

            if (adaptResult is null || adaptResult.Status == MapFileStatus.Removed)
            {
                item.SubItems[FileNameColumnIndex].Text = tag.OriginalFileName ?? "<unknown filename>";
            }
            else if (adaptResult.NewFileName is not null)
            {
                item.SubItems[FileNameColumnIndex].Text = adaptResult.NewFileName;
                item.SubItems[FileNameColumnIndex].ForeColor = Color.Blue;
            }

            item.SetImageIndex(tag, adaptResult);
        }

        public static int CompareTo(this ListViewItem item, ListViewItem other, int column)
        {
            return column switch
            {
                -1 => item.GetTag().OriginalIndex.CompareTo(other.GetTag().OriginalIndex),

                StatusColumnIndex => other.CompareStatus(item),

                _ => string.IsNullOrWhiteSpace(item.SubItems[column].Text) == string.IsNullOrWhiteSpace(other.SubItems[column].Text)
                    ? string.Compare(item.SubItems[column].Text, other.SubItems[column].Text, StringComparison.InvariantCulture)
                    : string.IsNullOrWhiteSpace(item.SubItems[column].Text) ? 1 : -1,
            };
        }

        private static int CompareStatus(this ListViewItem item, ListViewItem other)
        {
            var tag1 = item.GetTag();
            var tag2 = other.GetTag();

            var result = tag1.Status.CompareTo(tag2.Status);
            if (result != 0)
            {
                return result;
            }

            var modified1 = tag1.AdaptResult?.AdaptedFileStream is not null;
            var modified2 = tag2.AdaptResult?.AdaptedFileStream is not null;

            return modified1.CompareTo(modified2);
        }

        private static void SetImageIndex(this ListViewItem item, ItemTag tag, AdaptResult? adaptResult)
        {
            if (item.ListView is FileListView fileListView)
            {
                var severity = adaptResult?.Diagnostics is null
                    ? DiagnosticSeverity.Info
                    : adaptResult.Diagnostics.Select(d => d.Descriptor.Severity).Append(DiagnosticSeverity.Info).Max();

                if (severity == DiagnosticSeverity.Error)
                {
                    item.ImageIndex = fileListView.ErrorImageIndex;
                }
                else if (severity == DiagnosticSeverity.Warning)
                {
                    item.ImageIndex = fileListView.WarningImageIndex;
                }
                else
                {
                    item.ImageIndex = fileListView.GetImageIndexForStatus(tag.Status);
                }
            }
        }
    }
}