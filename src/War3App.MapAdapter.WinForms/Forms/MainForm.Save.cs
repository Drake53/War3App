using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.WinForms.Extensions;

namespace War3App.MapAdapter.WinForms.Forms
{
    partial class MainForm
    {
        private void SaveArchive(string fileName)
        {
            var itemCount = 0;
            for (var i = 0; i < _fileList.Items.Count; i++)
            {
                var mapFile = _fileList.Items[i].GetMapFile();
                if (mapFile.Status != MapFileStatus.Removed)
                {
                    itemCount++;
                }
            }

            _progressBar.Value = 0;
            _progressBar.Maximum = itemCount;
            _progressBar.CustomText = string.Empty;
            _progressBar.Visible = true;

            _saveArchiveWorker.RunWorkerAsync(fileName);
        }

        private BackgroundWorker CreateSaveArchiveWorker()
        {
            var worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false,
            };

            worker.DoWork += SaveArchiveBackgroundWork;
            worker.ProgressChanged += SaveArchiveProgressChanged;
            worker.RunWorkerCompleted += SaveArchiveCompleted;

            return worker;
        }

        private void SaveArchiveBackgroundWork(object? sender, DoWorkEventArgs e)
        {
            ArchiveProcessor.SaveArchive(
                _archive,
                _fileList.Items.Cast<ListViewItem>().Select(item => item.GetMapFile()),
                (string)e.Argument,
                _saveArchiveWorker);
        }

        private void SaveArchiveProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is SaveArchiveProgress saveArchiveProgress)
            {
                if (saveArchiveProgress.Saving)
                {
                    _progressBar.CustomText = ProgressText.Saving;
                }
                else
                {
                    _progressBar.Value++;
                    _progressBar.Value += e.ProgressPercentage;
                    _progressBar.CustomText = $"{_progressBar.Value} / {_progressBar.Maximum}";
                }
            }
        }

        private void SaveArchiveCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error is not null)
            {
                throw new ApplicationException(ExceptionText.SaveArchive, e.Error);
            }
            else
            {
                _progressBar.Visible = false;
            }
        }
    }
}