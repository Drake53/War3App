using System;
using System.ComponentModel;
using System.Linq;
using War3App.MapAdapter.Constants;

namespace War3App.MapAdapter.EtoForms.Forms
{
    partial class MainForm
    {
        private void SaveArchive(string outputFilePath)
        {
            var itemCount = 0;
            foreach (var item in _fileTree)
            {
                var mapFile = item.MapFile;
                if (mapFile.Status != MapFileStatus.Removed)
                {
                    itemCount++;
                }

                if (mapFile.Children is not null)
                {
                    itemCount += mapFile.Children.Length;
                }
            }

            _progressBar.Value = 0;
            _progressBar.Maximum = itemCount;
            _progressBar.CustomText = string.Empty;
            _progressBar.Visible = true;

            _saveArchiveWorker.RunWorkerAsync(outputFilePath);
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
                _fileTree.Select(item => item.MapFile),
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