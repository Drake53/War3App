using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using War3App.Common.WinForms;
using War3App.Common.WinForms.Extensions;

using War3Net.Build;
using War3Net.Build.Audio;
using War3Net.Build.Environment;
using War3Net.Build.Extensions;
using War3Net.Build.Import;
using War3Net.Build.Script;
using War3Net.Build.Widget;
using War3Net.CodeAnalysis.Jass;
using War3Net.IO.Mpq;

namespace War3App.MapUnlocker.WinForms
{
    internal static class MainForm
    {
        private const string Title = "Map Decompiler v0.1.0";

        private static MpqArchive _archive;
        private static Map _map;

        private static TextBox _archiveInput;
        private static Button _archiveInputBrowseButton;
        private static Button _openCloseArchiveButton;
        private static FileSystemWatcher _watcher;

        private static Button _saveAsButton;
        private static Button _autoDetectFilesToDecompileButton;
        private static CheckBox[] _filesToDecompileCheckBoxes;

        private static TextProgressBar _progressBar;
        private static BackgroundWorker _worker;

        [STAThread]
        private static void Main(string[] args)
        {
            _watcher = new FileSystemWatcher();
            _watcher.Created += OnWatchedFileEvent;
            _watcher.Renamed += OnWatchedFileEvent;
            _watcher.Deleted += OnWatchedFileEvent;

            var form = new Form();
            form.Text = Title;

            _archiveInput = new TextBox
            {
                PlaceholderText = "Input map...",
            };

            _openCloseArchiveButton = new Button
            {
                Text = "Open archive",
                Enabled = false,
            };

            _saveAsButton = new Button
            {
                Text = "Save As...",
            };

            _saveAsButton.Size = _saveAsButton.PreferredSize;
            _saveAsButton.Enabled = false;

            _autoDetectFilesToDecompileButton = new Button
            {
                Text = "Detect files to decompile",
            };

            _autoDetectFilesToDecompileButton.Size = _autoDetectFilesToDecompileButton.PreferredSize;
            _autoDetectFilesToDecompileButton.Enabled = false;

            _filesToDecompileCheckBoxes = new[]
            {
                new CheckBox
                {
                    Text = $"{nameof(MapFiles.Sounds)} ({MapSounds.FileName})",
                    Tag = MapFiles.Sounds,
                },

                new CheckBox
                {
                    Text = $"{nameof(MapFiles.Cameras)} ({MapCameras.FileName})",
                    Tag = MapFiles.Cameras,
                },

                new CheckBox
                {
                    Text = $"{nameof(MapFiles.Regions)} ({MapRegions.FileName})",
                    Tag = MapFiles.Regions,
                },

                new CheckBox
                {
                    Text = $"{nameof(MapFiles.Triggers)} ({MapTriggers.FileName})",
                    Tag = MapFiles.Triggers,
                },
            };

            foreach (var checkBox in _filesToDecompileCheckBoxes)
            {
                checkBox.Size = checkBox.PreferredSize;
                checkBox.Enabled = false;

                checkBox.CheckedChanged += (s, e) =>
                {
                    _saveAsButton.Enabled = _filesToDecompileCheckBoxes.Any(checkBox => checkBox.Checked);
                };
            }

            _archiveInput.TextChanged += OnArchiveInputTextChanged;

            _archiveInputBrowseButton = new Button
            {
                Text = "Browse",
            };

            _archiveInputBrowseButton.Size = _archiveInputBrowseButton.PreferredSize;
            _archiveInputBrowseButton.Click += (s, e) =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    CheckFileExists = false,
                };
                openFileDialog.Filter = string.Join('|', new[]
                {
                    "Warcraft III map|*.w3m;*.w3x",
                    "All files|*.*",
                });
                var openFileDialogResult = openFileDialog.ShowDialog();
                if (openFileDialogResult == DialogResult.OK)
                {
                    _archiveInput.Text = openFileDialog.FileName;
                }
            };

            _openCloseArchiveButton.Size = _openCloseArchiveButton.PreferredSize;
            _openCloseArchiveButton.Click += OnClickOpenCloseMap;

            _saveAsButton.Click += (s, e) =>
            {
                var saveFileDialog = new SaveFileDialog
                {
                    OverwritePrompt = true,
                    CreatePrompt = false,
                };

                var saveFileDialogResult = saveFileDialog.ShowDialog();
                if (saveFileDialogResult == DialogResult.OK)
                {
                    _progressBar.CustomText = "Starting...";
                    _progressBar.Value = 0;

                    _openCloseArchiveButton.Enabled = false;
                    _saveAsButton.Enabled = false;
                    _autoDetectFilesToDecompileButton.Enabled = false;
                    foreach (var checkBox in _filesToDecompileCheckBoxes)
                    {
                        checkBox.Enabled = false;
                    }

                    _progressBar.Visible = true;

                    _worker.RunWorkerAsync(saveFileDialog.FileName);
                }
            };

            _autoDetectFilesToDecompileButton.Click += (s, e) =>
            {
                var filesToDecompile = MapDecompiler.AutoDetectMapFilesToDecompile(_map);

                foreach (var checkBox in _filesToDecompileCheckBoxes)
                {
                    checkBox.Checked = filesToDecompile.HasFlag((MapFiles)checkBox.Tag);
                }
            };

            _progressBar = new TextProgressBar
            {
                Dock = DockStyle.Bottom,
                Style = ProgressBarStyle.Continuous,
                VisualMode = TextProgressBar.ProgressBarDisplayMode.CustomText,
                Visible = false,
            };

            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false,
            };

            _worker.DoWork += DecompileMapBackgroundWork;
            _worker.ProgressChanged += DecompileMapProgressChanged;
            _worker.RunWorkerCompleted += DecompileMapCompleted;

            // Initialize parser
            JassSyntaxFactory.ParseCompilationUnit(string.Empty);

            var flowLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
            };

            var inputArchiveFlowLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
            };

            var buttonsFlowLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
            };

            var checkBoxesFlowLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
            };

            inputArchiveFlowLayout.AddControls(_archiveInput, _archiveInputBrowseButton, _openCloseArchiveButton);
            buttonsFlowLayout.AddControls(_saveAsButton, _autoDetectFilesToDecompileButton);
            checkBoxesFlowLayout.AddControls(_filesToDecompileCheckBoxes);
            checkBoxesFlowLayout.Size = checkBoxesFlowLayout.PreferredSize;

            flowLayout.AddControls(inputArchiveFlowLayout, buttonsFlowLayout, checkBoxesFlowLayout);

            form.AddControls(flowLayout, _progressBar);

            form.SizeChanged += (s, e) =>
            {
                var width = form.Width;
                _archiveInput.Width = (width > 360 ? 360 : width) - 10;

                inputArchiveFlowLayout.Size = inputArchiveFlowLayout.GetPreferredSize(new Size(width, 0));
                buttonsFlowLayout.Size = buttonsFlowLayout.GetPreferredSize(new Size(width, 0));
                flowLayout.Height
                    = inputArchiveFlowLayout.Margin.Top + inputArchiveFlowLayout.Height + inputArchiveFlowLayout.Margin.Bottom
                    + buttonsFlowLayout.Margin.Top + buttonsFlowLayout.Height + buttonsFlowLayout.Margin.Bottom;
            };

            form.Size = new Size(400, 300);
            form.MinimumSize = new Size(400, 200);

            form.FormClosing += (s, e) =>
            {
                _archive?.Dispose();
                _worker?.Dispose();
            };

            form.ShowDialog();
        }

        private static void OnClickOpenCloseMap(object sender, EventArgs e)
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

        private static void OnArchiveInputTextChanged(object sender, EventArgs e)
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

        private static void OnWatchedFileEvent(object sender, EventArgs e)
        {
            SetOpenArchiveButtonEnabled(File.Exists(_archiveInput.Text));
        }

        private static void SetOpenArchiveButtonEnabled(bool enabled)
        {
            if (_openCloseArchiveButton.InvokeRequired)
            {
                _openCloseArchiveButton.Invoke(new Action(() => _openCloseArchiveButton.Enabled = enabled));
            }
            else
            {
                _openCloseArchiveButton.Enabled = enabled;
            }
        }

        private static void OpenArchive()
        {
            var fileInfo = new FileInfo(_archiveInput.Text);
            if (fileInfo.Exists)
            {
                _archiveInput.Enabled = false;
                _archiveInputBrowseButton.Enabled = false;
                _openCloseArchiveButton.Text = "Close archive";
                _saveAsButton.Enabled = _filesToDecompileCheckBoxes.Any(checkBox => checkBox.Checked);

                _archive = MpqArchive.Open(fileInfo.FullName, true);
                _archive.DiscoverFileNames();

                _map = Map.Open(_archive);

                _autoDetectFilesToDecompileButton.Enabled = true;
                foreach (var checkBox in _filesToDecompileCheckBoxes)
                {
                    checkBox.Enabled = true;
                }
            }
        }

        private static void CloseArchive()
        {
            _archive.Dispose();
            _archive = null;

            _map = null;

            _archiveInput.Enabled = true;
            _archiveInputBrowseButton.Enabled = true;

            _openCloseArchiveButton.Text = "Open archive";
            _saveAsButton.Enabled = false;

            _autoDetectFilesToDecompileButton.Enabled = false;
            foreach (var checkBox in _filesToDecompileCheckBoxes)
            {
                checkBox.Enabled = false;
            }
        }

        private static void DecompileMapBackgroundWork(object sender, DoWorkEventArgs e)
        {
            var outputFile = (string)e.Argument;

            var filesToDecompile = (MapFiles)0;
            foreach (var checkBox in _filesToDecompileCheckBoxes)
            {
                if (checkBox.Checked)
                {
                    filesToDecompile |= (MapFiles)checkBox.Tag;
                }
            }

            var decompiledMap = MapDecompiler.DecompileMap(_map, filesToDecompile, _worker);

            var mapBuilder = new MapBuilder(decompiledMap);
            mapBuilder.AddFiles(_archive);
            mapBuilder.Build(outputFile);
        }

        private static void DecompileMapProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _progressBar.CustomText = (string)e.UserState;
            _progressBar.Value = e.ProgressPercentage;
        }

        private static void DecompileMapCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _openCloseArchiveButton.Enabled = true;
            _saveAsButton.Enabled = true;
            _autoDetectFilesToDecompileButton.Enabled = true;
            foreach (var checkBox in _filesToDecompileCheckBoxes)
            {
                checkBox.Enabled = true;
            }

            _progressBar.Visible = false;
        }
    }
}