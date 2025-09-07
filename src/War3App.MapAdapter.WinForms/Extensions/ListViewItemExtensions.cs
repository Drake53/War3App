using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using War3App.MapAdapter.Constants;
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

        public static MapFile GetMapFile(this ListViewItem item)
        {
            return (MapFile)item.Tag;
        }

        public static void Update(this ListViewItem item)
        {
            var mapFile = item.GetMapFile();
            if (mapFile.Children is not null)
            {
                mapFile.Status = mapFile.Children.Max(child => child.Status);
            }

            item.SubItems[StatusColumnIndex].Text = mapFile.Status.ToString();
            item.SetImageIndex(mapFile, null);
        }

        public static void Update(this ListViewItem item, AdaptResult? adaptResult)
        {
            var mapFile = item.GetMapFile();

            item.SubItems[StatusColumnIndex].Text = mapFile.Status.ToString();
            item.SubItems[FileNameColumnIndex].ForeColor = adaptResult?.AdaptedFileStream is not null ? Color.Violet : Color.Black;

            if (adaptResult is null || adaptResult.Status == MapFileStatus.Removed)
            {
                item.SubItems[FileNameColumnIndex].Text = mapFile.OriginalFileName ?? MiscStrings.UnknownFileName;
            }
            else if (adaptResult.NewFileName is not null)
            {
                item.SubItems[FileNameColumnIndex].Text = adaptResult.NewFileName;
                item.SubItems[FileNameColumnIndex].ForeColor = Color.Blue;
            }

            item.SetImageIndex(mapFile, adaptResult);
        }

        public static int CompareTo(this ListViewItem item, ListViewItem other, int column)
        {
            return column switch
            {
                -1 => item.GetMapFile().OriginalIndex.CompareTo(other.GetMapFile().OriginalIndex),

                StatusColumnIndex => other.CompareStatus(item),

                _ => string.IsNullOrWhiteSpace(item.SubItems[column].Text) == string.IsNullOrWhiteSpace(other.SubItems[column].Text)
                    ? string.Compare(item.SubItems[column].Text, other.SubItems[column].Text, StringComparison.InvariantCulture)
                    : string.IsNullOrWhiteSpace(item.SubItems[column].Text) ? 1 : -1,
            };
        }

        private static int CompareStatus(this ListViewItem item, ListViewItem other)
        {
            var mapFile1 = item.GetMapFile();
            var mapFile2 = other.GetMapFile();

            var result = mapFile1.Status.CompareTo(mapFile2.Status);
            if (result != 0)
            {
                return result;
            }

            var modified1 = mapFile1.AdaptResult?.AdaptedFileStream is not null;
            var modified2 = mapFile2.AdaptResult?.AdaptedFileStream is not null;

            return modified1.CompareTo(modified2);
        }

        private static void SetImageIndex(this ListViewItem item, MapFile mapFile, AdaptResult? adaptResult)
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
                    item.ImageIndex = fileListView.GetImageIndexForStatus(mapFile.Status);
                }
            }
        }
    }
}