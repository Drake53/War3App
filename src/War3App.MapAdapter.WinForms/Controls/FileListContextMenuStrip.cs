using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.WinForms.Extensions;
using War3App.MapAdapter.WinForms.Forms;
using War3App.MapAdapter.WinForms.Resources;

namespace War3App.MapAdapter.WinForms.Controls
{
    [DesignerCategory("")]
    internal class FileListContextMenuStrip : ContextMenuStrip
    {
        private const int MaxItemWidth = 200;
        private const int MaxItemHeight = 20;

        private readonly FileListView _fileList;
        private readonly ToolStripButton _adaptContextButton;
        private readonly ToolStripButton _editContextButton;
        private readonly ToolStripButton _saveContextButton;
        private readonly ToolStripButton _diffContextButton;
        private readonly ToolStripButton _undoContextButton;
        private readonly ToolStripButton _removeContextButton;

        private int _itemWidth = MaxItemWidth;
        private int _itemHeight = MaxItemHeight;

        public FileListContextMenuStrip(FileListView fileList)
        {
            ShowImageMargin = false;

            _fileList = fileList;

            _adaptContextButton = new ToolStripButton(ButtonText.Adapt, Icons.Lightning);
            _editContextButton = new ToolStripButton(ButtonText.Edit, Icons.Modify);
            _saveContextButton = new ToolStripButton(ButtonText.SaveFile, Icons.Download);
            _diffContextButton = new ToolStripButton(ButtonText.Compare, Icons.Copy);
            _undoContextButton = new ToolStripButton(ButtonText.Undo, Icons.Undo);
            _removeContextButton = new ToolStripButton(ButtonText.Remove, Icons.Delete);

            Items.AddRange(new[]
            {
                _adaptContextButton,
                _editContextButton,
                _saveContextButton,
                _diffContextButton,
                _undoContextButton,
                _removeContextButton,
            });

            ResizeToFitItems();

            Opening += OnOpeningFileListContextMenu;
        }

        public event EventHandler? Adapt;

        public event EventHandler? Edit;

        public event EventHandler? Save;

        public event EventHandler? Diff;

        public event EventHandler? Undo;

        public event EventHandler? Remove;

        protected override Size MaxItemSize => new(_itemWidth, _itemHeight);

        public void SetMaxItemWidth(int width) => _itemWidth = width;

        public void RegisterClickEvents()
        {
            _adaptContextButton.Click += Adapt;
            _editContextButton.Click += Edit;
            _saveContextButton.Click += Save;
            _diffContextButton.Click += Diff;
            _undoContextButton.Click += Undo;
            _removeContextButton.Click += Remove;
        }

        private void ResizeToFitItems()
        {
            var width = Items.Cast<ToolStripItem>().Max(item => item.GetPreferredSize(MaxItemSize).Width) + Padding.Horizontal + Margin.Horizontal;

            SetMaxItemWidth(width);
            Width = width;
        }

        private void OnOpeningFileListContextMenu(object? sender, EventArgs e)
        {
            if (_fileList.TryGetSelectedMapFile(out var mapFile))
            {
                _adaptContextButton.Enabled = MainForm.Instance.CanAdapt && mapFile.Status == MapFileStatus.Pending;
                _editContextButton.Enabled = mapFile.Adapter?.IsTextFile == true;
                _saveContextButton.Enabled = mapFile.CurrentStream is not null && mapFile.Children is null;
                _diffContextButton.Enabled = mapFile.Adapter is not null && mapFile.AdaptResult?.AdaptedFileStream is not null && (mapFile.Adapter.IsTextFile || mapFile.Adapter.IsJsonSerializationSupported);
                _undoContextButton.Enabled = mapFile.Status == MapFileStatus.Removed || mapFile.AdaptResult?.AdaptedFileStream is not null;
                _removeContextButton.Enabled = mapFile.Status != MapFileStatus.Removed;
            }
            else
            {
                var mapFiles = _fileList.GetSelectedMapFiles();

                _adaptContextButton.Enabled = MainForm.Instance.CanAdapt && mapFiles.Any(mapFile => mapFile.Status == MapFileStatus.Pending);
                _editContextButton.Enabled = false;
                _saveContextButton.Enabled = false;
                _diffContextButton.Enabled = false;
                _undoContextButton.Enabled = mapFiles.Any(mapFile => mapFile.Status == MapFileStatus.Removed || mapFile.AdaptResult?.AdaptedFileStream is not null);
                _removeContextButton.Enabled = mapFiles.Any(mapFile => mapFile.Status != MapFileStatus.Removed);
            }
        }
    }
}