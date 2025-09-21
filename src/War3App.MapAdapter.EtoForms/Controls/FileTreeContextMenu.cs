using System;
using System.Linq;

using Eto.Forms;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.EtoForms.Constants;
using War3App.MapAdapter.EtoForms.Forms;
using War3App.MapAdapter.EtoForms.Helpers;
using War3App.MapAdapter.Extensions;

namespace War3App.MapAdapter.EtoForms.Controls
{
    internal class FileTreeContextMenu : ContextMenu
    {
        private readonly MainForm _mainForm;
        private readonly FileTreeView _fileTree;
        private readonly ButtonMenuItem _adaptContextButton;
        private readonly ButtonMenuItem _editContextButton;
        private readonly ButtonMenuItem _saveContextButton;
        private readonly ButtonMenuItem _diffContextButton;
        private readonly ButtonMenuItem _undoContextButton;
        private readonly ButtonMenuItem _removeContextButton;

        public FileTreeContextMenu(MainForm mainForm, FileTreeView fileTree)
        {
            _mainForm = mainForm;
            _fileTree = fileTree;

            _adaptContextButton = new ButtonMenuItem { Text = ButtonText.Adapt, Enabled = false, Image = Icons.Lightning, Style = Styles.MenuIcons };
            _editContextButton = new ButtonMenuItem { Text = ButtonText.Edit, Enabled = false, Image = Icons.Modify, Style = Styles.MenuIcons };
            _saveContextButton = new ButtonMenuItem { Text = ButtonText.SaveFile, Enabled = false, Image = Icons.Download, Style = Styles.MenuIcons };
            _diffContextButton = new ButtonMenuItem { Text = ButtonText.Compare, Enabled = false, Image = Icons.Copy, Style = Styles.MenuIcons };
            _undoContextButton = new ButtonMenuItem { Text = ButtonText.Undo, Enabled = false, Image = Icons.Undo, Style = Styles.MenuIcons };
            _removeContextButton = new ButtonMenuItem { Text = ButtonText.Remove, Enabled = false, Image = Icons.Delete, Style = Styles.MenuIcons };

            Items.AddRange(new[]
            {
                _adaptContextButton,
                _editContextButton,
                _saveContextButton,
                _diffContextButton,
                _undoContextButton,
                _removeContextButton,
            });

            Opening += OnOpeningFileTreeContextMenu;
        }

        public event EventHandler<EventArgs> Adapt
        {
            add => _adaptContextButton.Click += value;
            remove => _adaptContextButton.Click -= value;
        }

        public event EventHandler<EventArgs> Edit
        {
            add => _editContextButton.Click += value;
            remove => _editContextButton.Click -= value;
        }

        public event EventHandler<EventArgs> Save
        {
            add => _saveContextButton.Click += value;
            remove => _saveContextButton.Click -= value;
        }

        public event EventHandler<EventArgs> Diff
        {
            add => _diffContextButton.Click += value;
            remove => _diffContextButton.Click -= value;
        }

        public event EventHandler<EventArgs> Undo
        {
            add => _undoContextButton.Click += value;
            remove => _undoContextButton.Click -= value;
        }

        public event EventHandler<EventArgs> Remove
        {
            add => _removeContextButton.Click += value;
            remove => _removeContextButton.Click -= value;
        }

        private void OnOpeningFileTreeContextMenu(object? sender, EventArgs e)
        {
            // Defer the button state update to allow the GridView to update SelectedItems
            Application.Instance.AsyncInvoke(UpdateContextMenuButtonStates);
        }

        private void UpdateContextMenuButtonStates()
        {
            var canAdapt = _mainForm.CanAdapt;

            var mapFiles = _fileTree.GetSelectedMapFiles();
            if (mapFiles.Length == 1)
            {
                var mapFile = mapFiles[0];

                _adaptContextButton.Enabled = canAdapt && mapFile.Status == MapFileStatus.Pending;
                _editContextButton.Enabled = mapFile.Adapter?.IsTextFile == true;
                _saveContextButton.Enabled = mapFile.CurrentStream is not null && mapFile.Children is null;
                _diffContextButton.Enabled = mapFile.Adapter is not null && mapFile.AdaptResult?.AdaptedFileStream is not null && (mapFile.Adapter.IsTextFile || mapFile.Adapter.IsJsonSerializationSupported);
                _undoContextButton.Enabled = mapFile.CanUndoChanges();
                _removeContextButton.Enabled = mapFile.Status != MapFileStatus.Removed;
            }
            else
            {
                _adaptContextButton.Enabled = canAdapt && mapFiles.Any(mapFile => mapFile.Status == MapFileStatus.Pending);
                _editContextButton.Enabled = false;
                _saveContextButton.Enabled = false;
                _diffContextButton.Enabled = false;
                _undoContextButton.Enabled = mapFiles.Any(MapFileExtensions.CanUndoChanges);
                _removeContextButton.Enabled = mapFiles.Any(mapFile => mapFile.Status != MapFileStatus.Removed);
            }
        }
    }
}