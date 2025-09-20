using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.WinForms.Extensions;

using War3Net.Build.Extensions;
using War3Net.IO.Mpq;

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
            var archiveBuilder = new MpqArchiveBuilder(_archive);

            var progress = new SaveArchiveProgress();
            progress.Saving = false;

            for (var i = 0; i < _fileList.Items.Count; i++)
            {
                var mapFile = _fileList.Items[i].GetMapFile();
                if (mapFile.Parent is not null)
                {
                    continue;
                }

                if (mapFile.Status == MapFileStatus.Removed)
                {
                    if (mapFile.TryGetHashedFileName(out var hashedFileName))
                    {
                        archiveBuilder.RemoveFile(hashedFileName);
                    }
                    else
                    {
                        archiveBuilder.RemoveFile(_archive, mapFile.MpqEntry);
                    }
                }
                else if (mapFile.Children is not null)
                {
                    if (mapFile.Children.All(child => child.Status == MapFileStatus.Removed))
                    {
                        throw new InvalidOperationException(string.Format(ExceptionText.ChildrenRemoved, mapFile.Status));
                    }
                    else if (mapFile.Children.Any(child => child.IsModified || child.Status == MapFileStatus.Removed))
                    {
                        // Assume at most one nested archive (for campaign archives), so no recursion.
                        using var nestedArchive = MpqArchive.Open(_archive.OpenFile(mapFile.OriginalFileName));
                        foreach (var child in mapFile.Children)
                        {
                            if (child.OriginalFileName is not null)
                            {
                                nestedArchive.AddFileName(child.OriginalFileName);
                            }
                        }

                        var nestedArchiveBuilder = new MpqArchiveBuilder(nestedArchive);
                        foreach (var child in mapFile.Children)
                        {
                            if (child.Status == MapFileStatus.Removed)
                            {
                                if (child.TryGetHashedFileName(out var hashedFileName))
                                {
                                    nestedArchiveBuilder.RemoveFile(hashedFileName);
                                }
                                else
                                {
                                    nestedArchiveBuilder.RemoveFile(nestedArchive, child.MpqEntry);
                                }
                            }
                            else if (child.TryGetModifiedMpqFile(out var nestedArchiveAdaptedFile))
                            {
                                nestedArchiveBuilder.AddFile(nestedArchiveAdaptedFile);

                                _saveArchiveWorker.ReportProgress(0, progress);
                            }
                            else
                            {
                                _saveArchiveWorker.ReportProgress(0, progress);
                            }
                        }

                        var adaptedNestedArchiveStream = new MemoryStream();
                        nestedArchiveBuilder.SaveWithPreArchiveData(adaptedNestedArchiveStream, true);

                        adaptedNestedArchiveStream.Position = 0;
                        var adaptedFile = MpqFile.New(adaptedNestedArchiveStream, mapFile.CurrentFileName, false);
                        adaptedFile.TargetFlags = mapFile.MpqEntry.Flags;
                        archiveBuilder.AddFile(adaptedFile);

                        _saveArchiveWorker.ReportProgress(0, progress);
                    }
                    else
                    {
                        _saveArchiveWorker.ReportProgress(mapFile.Children.Length, progress);
                    }
                }
                else if (mapFile.TryGetModifiedMpqFile(out var adaptedFile))
                {
                    archiveBuilder.AddFile(adaptedFile);

                    _saveArchiveWorker.ReportProgress(0, progress);
                }
                else
                {
                    _saveArchiveWorker.ReportProgress(0, progress);
                }
            }

            progress.Saving = true;
            _saveArchiveWorker.ReportProgress(0, progress);

            using (var fileStream = File.Create((string)e.Argument))
            {
                archiveBuilder.SaveWithPreArchiveData(fileStream);
            }
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