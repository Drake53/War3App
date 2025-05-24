using System.ComponentModel;
using System.Windows.Forms;

using War3App.MapAdapter.WinForms.Extensions;
using War3App.MapAdapter.WinForms.Resources;

namespace War3App.MapAdapter.WinForms.Controls
{
    [DesignerCategory("")]
    public class FileListView : ListView
    {
        private readonly FileListSorter _fileListSorter;

        private readonly int _bubbleImageIndex;
        private readonly int _errorImageIndex;
        private readonly int _lockImageIndex;
        private readonly int _okImageIndex;
        private readonly int _questionImageIndex;
        private readonly int _trashImageIndex;
        private readonly int _warningImageIndex;

        public FileListView()
        {
            HeaderStyle = ColumnHeaderStyle.Clickable;
            Dock = DockStyle.Fill;
            View = View.Details;
            FullRowSelect = true;
            MultiSelect = true;

            _fileListSorter = new FileListSorter(this);

            ListViewItemSorter = _fileListSorter;
            ColumnClick += _fileListSorter.Sort;

            Columns.AddRange(new[]
            {
                new ColumnHeader { Text = "Status", Width = 102 },
                new ColumnHeader { Text = "FileName", Width = 300 },
                new ColumnHeader { Text = "FileType", Width = 130 },
                new ColumnHeader { Text = "Archive", Width = 87 },
            });

            SmallImageList = new ImageList();

            _bubbleImageIndex = SmallImageList.AddImage(Icons.Bubble);
            _errorImageIndex = SmallImageList.AddImage(Icons.Error);
            _lockImageIndex = SmallImageList.AddImage(Icons.Lock);
            _okImageIndex = SmallImageList.AddImage(Icons.OK);
            _questionImageIndex = SmallImageList.AddImage(Icons.Question);
            _trashImageIndex = SmallImageList.AddImage(Icons.Trash);
            _warningImageIndex = SmallImageList.AddImage(Icons.Warning);

            KeyDown += HandleKeyDown;
        }

        public int ErrorImageIndex => _errorImageIndex;

        public int WarningImageIndex => _warningImageIndex;

        public int GetImageIndexForStatus(MapFileStatus mapFileStatus)
        {
            return mapFileStatus switch
            {
                MapFileStatus.Removed => _trashImageIndex,
                MapFileStatus.Unknown => _questionImageIndex,
                MapFileStatus.Compatible => _okImageIndex,
                MapFileStatus.Inconclusive => _warningImageIndex,
                MapFileStatus.Pending => _bubbleImageIndex,
                MapFileStatus.Incompatible => _warningImageIndex,
                MapFileStatus.Locked => _lockImageIndex,
                MapFileStatus.Error => _errorImageIndex,
            };
        }

        public void Reset()
        {
            _fileListSorter.Reset();

            for (var i = 0; i < Items.Count; i++)
            {
                Items[i].GetTag().Dispose();
            }

            Items.Clear();
        }

        private void HandleKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                BeginUpdate();
                SelectedIndices.Clear();

                for (var i = 0; i < Items.Count; i++)
                {
                    SelectedIndices.Add(i);
                }

                EndUpdate();
            }
        }
    }
}