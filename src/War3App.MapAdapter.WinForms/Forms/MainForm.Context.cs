using System;
using System.IO;
using System.Windows.Forms;

using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.Extensions;
using War3App.MapAdapter.WinForms.Constants;
using War3App.MapAdapter.WinForms.Extensions;

namespace War3App.MapAdapter.WinForms.Forms
{
    partial class MainForm
    {
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
                        var adapter = childMapFile.Adapter;
                        if (adapter is not null && childMapFile.Status == MapFileStatus.Pending)
                        {
                            var context = new AdaptFileContext
                            {
                                FileName = childMapFile.CurrentFileName,
                                Archive = childMapFile.MpqArchive,
                                TargetPatch = _targetPatch,
                                OriginPatch = childMapFile.GetOriginPatch(_originPatch.Value),
                            };

                            mapFile.CurrentStream.Position = 0;
                            var adaptResult = adapter.Run(childMapFile.CurrentStream, context);
                            childMapFile.UpdateAdaptResult(adaptResult);
                            _fileList.UpdateItemForMapFile(childMapFile);
                        }
                    }

                    item.Update();
                }
                else
                {
                    var adapter = mapFile.Adapter;
                    if (adapter is not null && mapFile.Status == MapFileStatus.Pending)
                    {
                        var context = new AdaptFileContext
                        {
                            FileName = mapFile.CurrentFileName,
                            Archive = mapFile.MpqArchive,
                            TargetPatch = _targetPatch,
                            OriginPatch = mapFile.GetOriginPatch(_originPatch.Value),
                        };

                        mapFile.CurrentStream.Position = 0;
                        var adaptResult = adapter.Run(mapFile.CurrentStream, context);
                        mapFile.UpdateAdaptResult(adaptResult);
                        _fileList.UpdateItemForMapFile(mapFile);

                        if (mapFile.Parent is not null)
                        {
                            _fileList.GetItemByMapFile(mapFile.Parent).Update();
                        }
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
                else if (mapFile.Status != MapFileStatus.Removed)
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
    }
}