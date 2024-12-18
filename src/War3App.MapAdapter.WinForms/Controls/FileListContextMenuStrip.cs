using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using War3App.MapAdapter.WinForms.Extensions;
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

            _adaptContextButton = new ToolStripButton("Adapt");
            _adaptContextButton.Image = Icons.Lightning;

            _editContextButton = new ToolStripButton("Edit");
            _editContextButton.Image = Icons.Modify;

            _saveContextButton = new ToolStripButton("Save file");
            _saveContextButton.Image = Icons.Download;

            _diffContextButton = new ToolStripButton("Compare with unmodified");
            _diffContextButton.Image = Icons.Copy;

            _undoContextButton = new ToolStripButton("Undo changes");
            _undoContextButton.Image = Icons.Undo;

            _removeContextButton = new ToolStripButton("Remove");
            _removeContextButton.Image = Icons.Delete;

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

        public event EventHandler Adapt;

        public event EventHandler Edit;

        public event EventHandler Save;

        public event EventHandler Diff;

        public event EventHandler Undo;

        public event EventHandler Remove;

        protected override Size MaxItemSize => new(_itemWidth, _itemHeight);

        public void SetMaxItemWidth(int width) => _itemWidth = width;

        public void EnableClickEvents()
        {
            _adaptContextButton.Click += Adapt;
            _editContextButton.Click += Edit;
            _saveContextButton.Click += Save;
            _diffContextButton.Click += Diff;
            _undoContextButton.Click += Undo;
            _removeContextButton.Click += Remove;
        }

        public void ResizeToFitItems()
        {
            var width = Items.Cast<ToolStripItem>().Max(item => item.GetPreferredSize(MaxItemSize).Width) + Padding.Horizontal + Margin.Horizontal;

            SetMaxItemWidth(width);
            Width = width;
        }

        private void OnOpeningFileListContextMenu(object? sender, EventArgs e)
        {
            if (_fileList.TryGetSelectedItemTag(out var tag))
            {
                _adaptContextButton.Enabled = MainForm.TargetPatchSelected && tag.Status == MapFileStatus.Pending;
                _editContextButton.Enabled = tag.Adapter?.IsTextFile ?? false;
                _saveContextButton.Enabled = tag.Children is null;
                _diffContextButton.Enabled = tag.Adapter is not null && tag.AdaptResult?.AdaptedFileStream is not null && (tag.Adapter.IsTextFile || tag.Adapter.IsJsonSerializationSupported);
                _undoContextButton.Enabled = tag.Status == MapFileStatus.Removed || tag.AdaptResult?.AdaptedFileStream is not null;
                _removeContextButton.Enabled = tag.Status != MapFileStatus.Removed;
            }
            else
            {
                var tags = _fileList.GetSelectedItemTags();

                _adaptContextButton.Enabled = MainForm.TargetPatchSelected && tags.Any(tag => tag.Status == MapFileStatus.Pending);
                _editContextButton.Enabled = false;
                _saveContextButton.Enabled = false;
                _diffContextButton.Enabled = false;
                _undoContextButton.Enabled = tags.Any(tag => tag.Status == MapFileStatus.Removed || tag.AdaptResult?.AdaptedFileStream is not null);
                _removeContextButton.Enabled = tags.Any(tag => tag.Status != MapFileStatus.Removed);
            }
        }
    }
}