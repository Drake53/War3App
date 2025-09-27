using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.EtoForms.Extensions;
using War3App.MapAdapter.EtoForms.Models;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.EtoForms.Forms
{
    partial class MainForm
    {
        private void OpenArchive()
        {
            var fileInfo = new FileInfo(_archiveInput.Text);
            if (fileInfo.Exists)
            {
                _archiveInput.Enabled = false;
                _archiveInputBrowseButton.Enabled = false;
                _openCloseArchiveButton.Enabled = false;
                _openCloseArchiveButton.Text = ButtonText.Close;

                _progressBar.Value = 0;
                _progressBar.Maximum = 1;
                _progressBar.CustomText = string.Empty;
                _progressBar.Visible = true;

                _openArchiveWorker.RunWorkerAsync(fileInfo.FullName);
            }
        }

        private BackgroundWorker CreateOpenArchiveWorker()
        {
            var worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false,
            };

            worker.DoWork += OpenArchiveBackgroundWork;
            worker.ProgressChanged += OpenArchiveProgressChanged;
            worker.RunWorkerCompleted += OpenArchiveCompleted;

            return worker;
        }

        private void OpenArchiveBackgroundWork(object? sender, DoWorkEventArgs e)
        {
            var filePath = (string)e.Argument;

            if (string.Equals(Path.GetExtension(filePath), FileExtension.Zip, StringComparison.OrdinalIgnoreCase))
            {
                var zipArchive = ArchiveProcessor.OpenZipArchive(filePath);

                _archive = zipArchive.Archive;
                _targetPatch = zipArchive.TargetPatch;
                _isTargetPatchFromZipArchive = true;
            }
            else
            {
                _archive = MpqArchive.Open(filePath, true);
            }

            _archive.DiscoverFileNames();

            var result = ArchiveProcessor.OpenArchive(_archive, _openArchiveWorker);

            var fileTreeItems = new List<FileTreeItem>();
            var mapping = new Dictionary<MapFile, FileTreeItem>();

            foreach (var mapFile in result.Files)
            {
                var item = new FileTreeItem(mapFile);
                mapping.Add(mapFile, item);

                if (mapFile.Parent is null)
                {
                    fileTreeItems.Add(item);
                }
            }

            foreach (var item in fileTreeItems)
            {
                if (item.MapFile.Children is null)
                {
                    continue;
                }

                foreach (var childMapFile in item.MapFile.Children)
                {
                    item.Children.Add(mapping[childMapFile]);
                }

                item.UpdateStatus();
            }

            e.Result = fileTreeItems;

            _nestedArchives = result.NestedArchives;
            _originPatch = result.OriginPatch;
        }

        private void OpenArchiveProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is OpenArchiveProgress openArchiveProgress)
            {
                _progressBar.Value++;
                _progressBar.Maximum = openArchiveProgress.Maximum;
                _progressBar.CustomText = $"{_progressBar.Value} / {_progressBar.Maximum}";
            }
        }

        private void OpenArchiveCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error is not null)
            {
                throw new ApplicationException(ExceptionText.OpenArchive, e.Error);
            }
            else if (e.Result is List<FileTreeItem> fileTreeItems)
            {
                _fileTree.SuspendLayout();
                _fileTree.AddItems(fileTreeItems);
                _fileTree.ResumeLayout();

                if (!_isTargetPatchFromZipArchive)
                {
                    if (_targetPatchesComboBox.Items.Count > 1)
                    {
                        _targetPatchesComboBox.Enabled = true;
                    }
                    else
                    {
                        _targetPatch = GetTargetPatch(Enum.Parse<GamePatch>(_targetPatchesComboBox.Text.Replace('.', '_')));
                    }
                }

                _adaptAllButton.Enabled = CanAdapt && _fileTree.HasItems;
                _getHelpButton.Enabled = _targetPatch is not null;

                _openCloseArchiveButton.Enabled = true;
                _saveAsButton.Enabled = true;

                _progressBar.Visible = false;

                UpdateDiagnosticsDisplay();
            }
        }

        private void CloseArchive()
        {
            if (_archive is null)
            {
                return;
            }

            if (_nestedArchives is not null)
            {
                foreach (var nestedArchive in _nestedArchives)
                {
                    nestedArchive.Dispose();
                }

                _nestedArchives.Clear();
                _nestedArchives = null;
            }

            _archive.Dispose();
            _archive = null;
            _isTargetPatchFromZipArchive = false;

            _archiveInput.Enabled = true;
            _archiveInputBrowseButton.Enabled = true;

            _openCloseArchiveButton.Text = ButtonText.Open;
            _adaptAllButton.Enabled = false;
            _saveAsButton.Enabled = false;
            _getHelpButton.Enabled = false;

            _targetPatchesComboBox.Enabled = false;
            _targetPatch = null;
            _originPatch = null;

            _fileTree.Reset();

            _diagnosticsDisplay.Clear();

            GC.Collect();
        }
    }
}