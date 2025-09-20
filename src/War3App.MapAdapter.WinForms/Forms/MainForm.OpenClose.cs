using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.WinForms.Extensions;
using War3App.MapAdapter.WinForms.Helpers;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.WinForms.Forms
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
            var listViewItems = new List<ListViewItem>();

            foreach (var mapFile in result.Files)
            {
                var item = ListViewItemFactory.Create(mapFile, _fileList);
                if (mapFile.Parent is not null)
                {
                    item.IndentCount = 1;
                }
            }

            e.Result = listViewItems;

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
            else if (e.Result is List<ListViewItem> listViewItems)
            {
                _fileList.BeginUpdate();
                _fileList.Items.AddRange(listViewItems.ToArray());

                foreach (ListViewItem item in _fileList.Items)
                {
                    item.Update();
                }

                _fileList.EndUpdate();

                if (!_isTargetPatchFromZipArchive)
                {
                    if (_targetPatchesComboBox.Items.Count > 1)
                    {
                        _targetPatchesComboBox.Enabled = true;
                    }
                    else
                    {
                        _targetPatch = GetTargetPatch((GamePatch?)_targetPatchesComboBox.SelectedItem);
                    }
                }

                _adaptAllButton.Enabled = CanAdapt && _fileList.Items.Count > 0;
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

            _fileList.Reset();

            _diagnosticsDisplay.Text = string.Empty;

            GC.Collect();
        }
    }
}