using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using War3App.Common.WinForms;
using War3App.Common.WinForms.Extensions;

using War3Net.Build;
using War3Net.Build.Extensions;
using War3Net.Build.Info;
using War3Net.IO.Mpq;

namespace War3App.MapTranspiler.WinForms
{
    internal static class MainForm
    {
        private const string Title = "Map Transpiler v0.1.0";

        private static readonly string[] _progressBarSteps = new[]
        {
            "Registering common.j...",
            "Registering Blizzard.j...",
            "Transpiling script...",
            "Done",
        };

        private static MpqArchive _archive;

        private static TextBox _archiveInput;
        private static Button _archiveInputBrowseButton;
        private static Button _openCloseArchiveButton;
        private static FileSystemWatcher _watcher;

        private static Button _saveAsButton;
        private static ComboBox _targetScriptLanguagesComboBox;
        private static ScriptLanguage? _targetScriptLanguage;

        private static int _progressStepIndex;
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

            _targetScriptLanguagesComboBox = new ComboBox
            {
                Enabled = false,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120,
            };

            _targetScriptLanguagesComboBox.SelectedIndexChanged += (s, e) =>
            {
                _targetScriptLanguage = (ScriptLanguage?)_targetScriptLanguagesComboBox.SelectedItem;
                _saveAsButton.Enabled = _targetScriptLanguage.HasValue;
            };

            _targetScriptLanguagesComboBox.FormattingEnabled = true;
            _targetScriptLanguagesComboBox.Format += (s, e) =>
            {
                if (e.ListItem is ScriptLanguage scriptLanguage)
                {
                    e.Value = scriptLanguage.ToString();
                }
            };

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
                    _progressStepIndex = 0;

                    _progressBar.CustomText = _progressBarSteps[_progressStepIndex];
                    _progressBar.Value = 0;

                    _openCloseArchiveButton.Enabled = false;
                    _saveAsButton.Enabled = false;
                    _targetScriptLanguagesComboBox.Enabled = false;
                    _progressBar.Visible = true;

                    _worker.RunWorkerAsync(saveFileDialog.FileName);
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

            _worker.DoWork += TranspileMapBackgroundWork;
            _worker.ProgressChanged += TranspileMapProgressChanged;
            _worker.RunWorkerCompleted += TranspileMapCompleted;

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

            var targetScriptLanguageLabel = new Label
            {
                Text = "Transpile to",
                TextAlign = ContentAlignment.BottomRight,
            };

            inputArchiveFlowLayout.AddControls(_archiveInput, _archiveInputBrowseButton, _openCloseArchiveButton);
            buttonsFlowLayout.AddControls(_saveAsButton, targetScriptLanguageLabel, _targetScriptLanguagesComboBox);
            flowLayout.AddControls(inputArchiveFlowLayout, buttonsFlowLayout);

            form.AddControls(flowLayout, _progressBar);

            targetScriptLanguageLabel.Size = targetScriptLanguageLabel.PreferredSize;

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
                _saveAsButton.Enabled = true;

                _archive = MpqArchive.Open(fileInfo.FullName, true);
                _archive.DiscoverFileNames();

                var map = Map.Open(_archive);
                // _originScriptLanguage = map.Info.ScriptLanguage;

                var targetScriptLanguages = new HashSet<object>(new object[]
                {
                    // ScriptLanguage.Jass,
                    ScriptLanguage.Lua,
                });

                _targetScriptLanguagesComboBox.Items.AddRange(targetScriptLanguages.OrderByDescending(patch => (int)patch).ToArray());
                _targetScriptLanguagesComboBox.Enabled = true;
                if (_targetScriptLanguagesComboBox.Items.Count == 1)
                {
                    _targetScriptLanguagesComboBox.SelectedIndex = 0;
                }
            }
        }

        private static void CloseArchive()
        {
            _archive.Dispose();
            _archive = null;

            _archiveInput.Enabled = true;
            _archiveInputBrowseButton.Enabled = true;

            _openCloseArchiveButton.Text = "Open archive";
            _saveAsButton.Enabled = false;

            _targetScriptLanguagesComboBox.Enabled = false;
            _targetScriptLanguagesComboBox.SelectedIndex = -1;
            _targetScriptLanguagesComboBox.Items.Clear();
            // _originScriptLanguage = null;
        }

        private static void TranspileMapBackgroundWork(object sender, DoWorkEventArgs e)
        {
            var fileName = (string)e.Argument;
            MapScriptTranspiler.TranspileAndSave(_archiveInput.Text, fileName, null, null, _targetScriptLanguage.Value, _worker);
        }

        private static void TranspileMapProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _progressBar.CustomText = _progressBarSteps[++_progressStepIndex];
            _progressBar.Value = e.ProgressPercentage;
        }

        private static void TranspileMapCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _openCloseArchiveButton.Enabled = true;
            _saveAsButton.Enabled = true;
            _targetScriptLanguagesComboBox.Enabled = true;
            _progressBar.Visible = false;
        }
    }
}