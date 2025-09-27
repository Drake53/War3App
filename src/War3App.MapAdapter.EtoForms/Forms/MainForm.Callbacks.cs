using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

using Eto.Forms;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.EtoForms.Extensions;
using War3App.MapAdapter.EtoForms.Helpers;
using War3App.MapAdapter.EtoForms.Models;
using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;

namespace War3App.MapAdapter.EtoForms.Forms
{
    partial class MainForm
    {
        private void OnArchiveInputTextChanged(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_archiveInput.Text))
            {
                _watcher.EnableRaisingEvents = false;
                _openCloseArchiveButton.Enabled = false;
            }
            else
            {
                var fileInfo = new FileInfo(_archiveInput.Text);
                _watcher.Path = fileInfo.DirectoryName;
                _watcher.Filter = fileInfo.Name;
                _watcher.EnableRaisingEvents = true;

                _openCloseArchiveButton.Enabled = fileInfo.Exists;
            }
        }

        private void OnClickBrowseInputArchive(object? sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = TitleText.OpenFileOrFolderDialog,
                CheckFileExists = true,
            };

            foreach (var filter in GetMpqArchiveFileTypeFilters(isOpenFileDialog: true))
            {
                openFileDialog.Filters.Add(filter);
            }

            var openFileDialogResult = openFileDialog.ShowDialog(this);
            if (openFileDialogResult == DialogResult.Ok)
            {
                _archiveInput.Text = openFileDialog.FileName;
            }
        }

        private void OnClickOpenCloseArchive(object? sender, EventArgs e)
        {
            if (_archive is null)
            {
                OpenArchive();
            }
            else
            {
                CloseArchive();
            }
        }

        private void OnSelectedTargetPatchChanged(object? sender, EventArgs e)
        {
            if (_targetPatchesComboBox.SelectedValue is ListItem selectedItem &&
                Enum.TryParse<GamePatch>(selectedItem.Key, out var patch))
            {
                _targetPatch = GetTargetPatch(patch);
                _adaptAllButton.Enabled = CanAdapt && _fileTree.HasItems;
                _getHelpButton.Enabled = _targetPatch is not null;
            }
        }

        private void OnClickAdaptAll(object? sender, EventArgs e)
        {
            if (!CanAdapt)
            {
                return;
            }

            var adaptedItems = new HashSet<FileTreeItem>();

            _targetPatchesComboBox.Enabled = false;

            var selection = FileTreeHelper.GetSelection(_fileTree);

            foreach (var item in selection.Leafs)
            {
                var adaptResult = AdaptMapFile(item.MapFile);
                if (adaptResult.HasDiagnostics(minimumSeverity: DiagnosticSeverity.Warning))
                {
                    adaptedItems.Add(item);
                }
            }

            foreach (var parent in selection.Parents)
            {
                parent.UpdateStatus();
            }

            UpdateDiagnosticsDisplay();

            if (!_fileTree.SelectedItems.Any())
            {
                _fileTree.SelectedItems = adaptedItems;
            }

            _fileTree.Invalidate();
        }

        private void OnClickSaveAs(object? sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = TitleText.SaveFileDialog,
                FileName = $"{Path.GetFileNameWithoutExtension(_archiveInput.Text)}{MiscStrings.AdaptedFileTag}{Path.GetExtension(_archiveInput.Text)}",
                CheckFileExists = true,
            };

            foreach (var filter in GetMpqArchiveFileTypeFilters(isOpenFileDialog: false))
            {
                saveFileDialog.Filters.Add(filter);
            }

            var saveFileDialogResult = saveFileDialog.ShowDialog(this);
            if (saveFileDialogResult == DialogResult.Ok)
            {
                SaveArchive(saveFileDialog.FileName);
            }
        }

        private void OnClickGetHelp(object? sender, EventArgs e)
        {
            MessageBox.Show(this, "Not yet implemented.", "Info", MessageBoxType.Information);
        }

        private void OnFileKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Delete)
            {
                OnClickRemoveSelected(sender, e);
                e.Handled = true;
            }
        }

        private void OnFileSelectionChanged(object? sender, EventArgs e)
        {
            UpdateDiagnosticsDisplay();
        }

        private void OnClickAdaptSelected(object? sender, EventArgs e)
        {
            if (!CanAdapt)
            {
                return;
            }

            _targetPatchesComboBox.Enabled = false;

            var selection = FileTreeHelper.GetSelection(_fileTree.GetSelectedItems());

            foreach (var item in selection.Leafs)
            {
                _ = AdaptMapFile(item.MapFile);
            }

            foreach (var item in selection.Parents)
            {
                item.UpdateStatus();
            }

            UpdateDiagnosticsDisplay();

            _fileTree.Invalidate();
        }

        private void OnClickEditSelected(object? sender, EventArgs e)
        {
            MessageBox.Show(this, "Not yet implemented.", "Info", MessageBoxType.Information);
        }

        private void OnClickSaveSelected(object? sender, EventArgs e)
        {
            if (!_fileTree.TryGetSelectedMapFile(out var mapFile) ||
                mapFile.CurrentStream is null ||
                mapFile.Children is not null)
            {
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Title = TitleText.SaveFileDialog,
                FileName = Path.GetFileName(mapFile.CurrentFileName) ?? mapFile.Adapter?.DefaultFileName,
                CheckFileExists = true,
            };

            var extension = Path.GetExtension(saveFileDialog.FileName);
            if (!string.IsNullOrEmpty(extension))
            {
                var fileTypeDescription = mapFile.Adapter?.MapFileDescription ?? $"{extension.TrimStart('.').ToUpperInvariant()} file";

                saveFileDialog.Filters.Add(new FileFilter(fileTypeDescription, extension));
            }

            saveFileDialog.Filters.Add(new FileFilter(FileType.Any, FileExtension.AnyFile));

            var saveFileDialogResult = saveFileDialog.ShowDialog(this);
            if (saveFileDialogResult == DialogResult.Ok)
            {
                using var fileStream = File.Create(saveFileDialog.FileName);

                mapFile.CurrentStream.Position = 0;
                mapFile.CurrentStream.CopyTo(fileStream);
            }
        }

        private void OnClickDiffSelected(object? sender, EventArgs e)
        {
            MessageBox.Show(this, "Not yet implemented.", "Info", MessageBoxType.Information);
        }

        private void OnClickUndoChangesSelected(object? sender, EventArgs e)
        {
            foreach (var item in _fileTree.GetSelectedItems())
            {
                var mapFile = item.MapFile;

                if (mapFile.AdaptResult?.AdaptedFileStream is not null)
                {
                    mapFile.AdaptResult.Dispose();
                }
                else if (mapFile.Status != MapFileStatus.Removed && mapFile.AdaptResult?.NewFileName is null)
                {
                    continue;
                }

                mapFile.AdaptResult = null;
            }

            UpdateDiagnosticsDisplay();

            _fileTree.Invalidate();
        }

        private void OnClickRemoveSelected(object? sender, EventArgs e)
        {
            foreach (var item in _fileTree.GetSelectedItems())
            {
                var mapFile = item.MapFile;

                if (mapFile.Status == MapFileStatus.Removed)
                {
                    continue;
                }

                mapFile.AdaptResult?.Dispose();
                mapFile.AdaptResult = MapFileStatus.Removed;
            }

            UpdateDiagnosticsDisplay();

            _fileTree.Invalidate();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_nestedArchives is not null)
            {
                foreach (var nestedArchive in _nestedArchives)
                {
                    nestedArchive.Dispose();
                }
            }

            _archive?.Dispose();
            _openArchiveWorker?.Dispose();
            _saveArchiveWorker?.Dispose();
            _watcher?.Dispose();

            base.OnClosing(e);
        }
    }
}