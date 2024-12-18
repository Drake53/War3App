using System.Collections;
using System.Windows.Forms;

using War3App.MapAdapter.WinForms.Extensions;

namespace War3App.MapAdapter.WinForms
{
    internal sealed class FileListSorter : IComparer
    {
        private readonly ListView _fileList;

        public FileListSorter(ListView fileList)
        {
            _fileList = fileList;
        }

        public SortOrder SortOrder { get; set; }

        public int SortColumn { get; set; }

        public void Sort(object? sender, ColumnClickEventArgs e)
        {
            if (e.Column == SortColumn)
            {
                SortOrder = (SortOrder)(((int)SortOrder + 1) % 3);
            }
            else
            {
                SortOrder = SortOrder.Ascending;
                SortColumn = e.Column;
            }

            _fileList.Sort();
        }

        public int Compare(object? x, object? y)
        {
            if (x is ListViewItem item1 && y is ListViewItem item2)
            {
                return SortOrder switch
                {
                    SortOrder.None => item1.CompareTo(item2, -1),
                    SortOrder.Ascending => item1.CompareTo(item2, SortColumn),
                    SortOrder.Descending => 0 - item1.CompareTo(item2, SortColumn),
                };
            }

            return -1;
        }

        public void Reset()
        {
            SortOrder = SortOrder.None;
            SortColumn = default;
        }
    }
}