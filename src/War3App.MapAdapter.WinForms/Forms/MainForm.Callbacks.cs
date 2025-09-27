using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.Extensions;
using War3App.MapAdapter.WinForms.Constants;
using War3App.MapAdapter.WinForms.Extensions;

using War3Net.Build.Common;

namespace War3App.MapAdapter.WinForms.Forms
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

                SetOpenArchiveButtonEnabled(fileInfo.Exists);
            }
        }

        private void OnClickBrowseInputArchive(object? sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                CheckFileExists = false,
            };

            openFileDialog.Filter = GetMpqArchiveFileTypeFilter(isOpenFileDialog: true);

            var openFileDialogResult = openFileDialog.ShowDialog();
            if (openFileDialogResult == DialogResult.OK)
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

        private void FormatTargetPatch(object? sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is GamePatch gamePatch)
            {
                e.Value = gamePatch.PrettyPrint();
            }
        }

        private void OnSelectedTargetPatchChanged(object? sender, EventArgs e)
        {
            _targetPatch = GetTargetPatch((GamePatch?)_targetPatchesComboBox.SelectedItem);
            _adaptAllButton.Enabled = CanAdapt && _fileList.Items.Count > 0;
            _getHelpButton.Enabled = _targetPatch is not null;
        }

        private void OnClickAdaptAll(object? sender, EventArgs e)
        {
            if (!CanAdapt)
            {
                return;
            }

            var adaptedItemIndices = new List<int>();

            _targetPatchesComboBox.Enabled = false;
            var parentsToUpdate = new HashSet<MapFile>();
            for (var i = 0; i < _fileList.Items.Count; i++)
            {
                var item = _fileList.Items[i];
                var mapFile = item.GetMapFile();

                var adaptResult = AdaptMapFile(mapFile);
                if (adaptResult is not null)
                {
                    if (mapFile.Parent is not null)
                    {
                        parentsToUpdate.Add(mapFile.Parent);
                    }

                    if (adaptResult.Diagnostics is not null &&
                        adaptResult.Diagnostics.Length > 0)
                    {
                        adaptedItemIndices.Add(i);
                    }
                }
            }

            foreach (var parent in parentsToUpdate)
            {
                _fileList.GetItemByMapFile(parent).Update();
            }

            if (_fileList.SelectedItems.Count == 0)
            {
                _fileList.BeginUpdate();
                foreach (var itemIndex in adaptedItemIndices)
                {
                    _fileList.SelectedIndices.Add(itemIndex);
                }

                _fileList.EndUpdate();
            }

            UpdateDiagnosticsDisplay();
        }

        private void OnClickSaveAs(object? sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                OverwritePrompt = true,
                CreatePrompt = false,
                FileName = $"{Path.GetFileNameWithoutExtension(_archiveInput.Text)}{MiscStrings.AdaptedFileTag}{Path.GetExtension(_archiveInput.Text)}",
            };

            saveFileDialog.Filter = GetMpqArchiveFileTypeFilter(isOpenFileDialog: false);

            var saveFileDialogResult = saveFileDialog.ShowDialog();
            if (saveFileDialogResult == DialogResult.OK)
            {
                SaveArchive(saveFileDialog.FileName);
            }
        }

        private void OnClickGetHelp(object? sender, EventArgs e)
        {
            if (_targetPatch is not null)
            {
                _ = new GetHelpForm(_archiveInput.Text, _targetPatch).ShowDialog();
            }
        }

        private void OnFileKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                OnClickRemoveSelected(sender, e);
                e.Handled = true;
            }
        }

        private void OnFileSelectionChanged(object? sender, EventArgs e)
        {
            if (_fileSelectionChangedEventTimer is null)
            {
                _fileSelectionChangedEventTimer = new Timer();
                _fileSelectionChangedEventTimer.Tick += OnFileSelectionEventTimerTick;
                _fileSelectionChangedEventTimer.Interval = 50;
            }

            // Start/reset the timer
            _fileSelectionChangedEventTimer.Enabled = false;
            _fileSelectionChangedEventTimer.Enabled = true;
        }

        private void OnFileSelectionEventTimerTick(object? sender, EventArgs e)
        {
            _fileSelectionChangedEventTimer.Enabled = false;

            UpdateDiagnosticsDisplay();
        }

        private void OnClickAdaptSelected(object? sender, EventArgs e)
        {
            if (!CanAdapt)
            {
                return;
            }

            _targetPatchesComboBox.Enabled = false;
            for (var i = 0; i < _fileList.SelectedIndices.Count; i++)
            {
                var index = _fileList.SelectedIndices[i];
                var item = _fileList.Items[index];
                var mapFile = item.GetMapFile();

                if (mapFile.Children is not null)
                {
                    foreach (var childMapFile in mapFile.Children)
                    {
                        _ = AdaptMapFile(childMapFile);
                    }

                    item.Update();
                }
                else
                {
                    var adaptResult = AdaptMapFile(mapFile);
                    if (adaptResult is not null && mapFile.Parent is not null)
                    {
                        _fileList.GetItemByMapFile(mapFile.Parent).Update();
                    }
                }
            }

            UpdateDiagnosticsDisplay();
        }

        private void OnClickEditSelected(object? sender, EventArgs e)
        {
            if (!_fileList.TryGetSelectedMapFile(out var mapFile) ||
                mapFile.Adapter is null ||
                !mapFile.Adapter.IsTextFile)
            {
                return;
            }

            var scriptEditForm = new ScriptEditForm(mapFile.AdaptResult?.Diagnostics ?? Array.Empty<Diagnostic>());

            mapFile.CurrentStream.Position = 0;
            using (var reader = new StreamReader(mapFile.CurrentStream, leaveOpen: true))
            {
                scriptEditForm.Text = reader.ReadToEnd();
            }

            if (scriptEditForm.Show() == DialogResult.OK)
            {
                var memoryStream = new MemoryStream();
                using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
                {
                    writer.Write(scriptEditForm.Text);
                }

                memoryStream.Position = 0;
                mapFile.AdaptResult = AdaptResult.ModifiedByUser(memoryStream);
                _fileList.UpdateItemForMapFile(mapFile);

                _diagnosticsDisplay.Text = string.Empty;
            }
        }

        private void OnClickSaveSelected(object? sender, EventArgs e)
        {
            if (!_fileList.TryGetSelectedMapFile(out var mapFile) ||
                mapFile.CurrentStream is null ||
                mapFile.Children is not null)
            {
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                OverwritePrompt = true,
                CreatePrompt = false,
                FileName = Path.GetFileName(mapFile.CurrentFileName) ?? mapFile.Adapter?.DefaultFileName,
            };

            var extension = Path.GetExtension(saveFileDialog.FileName);
            if (string.IsNullOrEmpty(extension))
            {
                saveFileDialog.Filter = FilterStrings.AllFiles;
            }
            else
            {
                var fileTypeDescription = mapFile.Adapter?.MapFileDescription ?? $"{extension.TrimStart('.').ToUpperInvariant()} file";

                saveFileDialog.Filter = FilterStrings.Combine($"{fileTypeDescription}|*{extension}", FilterStrings.AllFiles);
            }

            var saveFileDialogResult = saveFileDialog.ShowDialog();
            if (saveFileDialogResult == DialogResult.OK)
            {
                using var fileStream = File.Create(saveFileDialog.FileName);

                mapFile.CurrentStream.Position = 0;
                mapFile.CurrentStream.CopyTo(fileStream);
            }
        }

        private void OnClickDiffSelected(object? sender, EventArgs e)
        {
            if (!_fileList.TryGetSelectedMapFile(out var mapFile) ||
                mapFile.Adapter is null ||
                mapFile.AdaptResult?.AdaptedFileStream is null)
            {
                return;
            }

            string oldText, newText;
            if (mapFile.Adapter.IsTextFile)
            {
                mapFile.OriginalFileStream.Position = 0;
                mapFile.AdaptResult.AdaptedFileStream.Position = 0;

                using var oldStreamReader = new StreamReader(mapFile.OriginalFileStream, leaveOpen: true);
                using var newStreamReader = new StreamReader(mapFile.AdaptResult.AdaptedFileStream, leaveOpen: true);

                oldText = oldStreamReader.ReadToEnd();
                newText = newStreamReader.ReadToEnd();
            }
            else if (mapFile.Adapter.IsJsonSerializationSupported && _targetPatch is not null)
            {
                mapFile.OriginalFileStream.Position = 0;
                mapFile.AdaptResult.AdaptedFileStream.Position = 0;

                oldText = mapFile.Adapter.GetJson(mapFile.OriginalFileStream, mapFile.GetOriginPatch(_originPatch.Value));
                newText = mapFile.Adapter.GetJson(mapFile.AdaptResult.AdaptedFileStream, _targetPatch.Patch);
            }
            else
            {
                return;
            }

            const int CharacterLimit = 25000;

            if (oldText.Length > CharacterLimit + 100)
            {
                oldText = oldText[..CharacterLimit] + $"{System.Environment.NewLine}{System.Environment.NewLine}FILE TOO LARGE: ONLY SHOWING FIRST {CharacterLimit}/{oldText.Length} CHARACTERS";
            }

            if (newText.Length > CharacterLimit + 100)
            {
                newText = newText[..CharacterLimit] + $"{System.Environment.NewLine}{System.Environment.NewLine}FILE TOO LARGE: ONLY SHOWING FIRST {CharacterLimit}/{newText.Length} CHARACTERS";
            }

            var diffForm = new DiffForm(oldText, newText);
            diffForm.ShowDialog();
        }

        private void OnClickUndoChangesSelected(object? sender, EventArgs e)
        {
            for (var i = 0; i < _fileList.SelectedItems.Count; i++)
            {
                var item = _fileList.SelectedItems[i];
                var mapFile = item.GetMapFile();

                if (mapFile.AdaptResult?.AdaptedFileStream is not null)
                {
                    mapFile.AdaptResult.Dispose();
                }
                else if (mapFile.Status != MapFileStatus.Removed && mapFile.AdaptResult?.NewFileName is null)
                {
                    continue;
                }

                mapFile.AdaptResult = null;
                item.Update(mapFile.AdaptResult);
            }
        }

        private void OnClickRemoveSelected(object? sender, EventArgs e)
        {
            for (var i = 0; i < _fileList.SelectedItems.Count; i++)
            {
                var item = _fileList.SelectedItems[i];
                var mapFile = item.GetMapFile();

                if (mapFile.Status == MapFileStatus.Removed)
                {
                    continue;
                }

                mapFile.AdaptResult?.Dispose();
                mapFile.AdaptResult = MapFileStatus.Removed;

                item.Update(mapFile.AdaptResult);
            }
        }

        private void DisposeOnClosing(object? sender, EventArgs e)
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
            _fileSelectionChangedEventTimer?.Dispose();
        }
    }
}