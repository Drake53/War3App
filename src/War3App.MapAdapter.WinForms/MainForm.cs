﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

using Microsoft.Extensions.Configuration;

using War3App.Common.WinForms;
using War3App.Common.WinForms.Extensions;
using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.Extensions;
using War3App.MapAdapter.Info;
using War3App.MapAdapter.WinForms.Controls;
using War3App.MapAdapter.WinForms.Extensions;
using War3App.MapAdapter.WinForms.Forms;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Info;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.WinForms
{
    internal static class MainForm
    {
        private const string Title = "Map Adapter v{0}";

        private static readonly GamePatch _latestPatch = Enum.GetValues<GamePatch>().Max();

        private static MpqArchive _archive;

        private static TextBox _archiveInput;
        private static Button _archiveInputBrowseButton;
        private static Button _openCloseArchiveButton;
        private static FileSystemWatcher _watcher;

        private static Button _adaptAllButton;
        private static Button _saveAsButton;
        private static ComboBox _targetPatchesComboBox;
        private static GamePatch? _targetPatch;
        private static GamePatch? _originPatch;
        private static Button _getHelpButton;

        private static FileListView _fileList;

        private static Timer? _fileSelectionChangedEventTimer;

        private static RichTextBox _diagnosticsDisplay;

        private static TextProgressBar _progressBar;
        private static BackgroundWorker _openArchiveWorker;
        private static BackgroundWorker _saveArchiveWorker;

        private static IConfiguration _configuration;
        private static AppSettings _appSettings;

        internal static bool TargetPatchSelected => _targetPatch.HasValue;

        [STAThread]
        private static void Main(string[] args)
        {
            if (!File.Exists("appsettings.json"))
            {
                var initialSetupDialog = new ConfigureGamePathForm();

                var initialSetupDialogResult = initialSetupDialog.ShowDialog();
                if (initialSetupDialogResult != DialogResult.OK)
                {
                    return;
                }

                var appSettings = new AppSettings
                {
                    TargetPatches = new List<TargetPatch>()
                    {
                        new TargetPatch
                        {
                            GameDataPath = initialSetupDialog.GameDirectory,
                            Patch = initialSetupDialog.GamePatch,
                        },
                    },
                };

                var jsonSerializerOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                File.WriteAllText("appsettings.json", JsonSerializer.Serialize(appSettings, jsonSerializerOptions));
            }

            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            ReloadSettings();

            _watcher = new FileSystemWatcher();
            _watcher.Created += OnWatchedFileEvent;
            _watcher.Renamed += OnWatchedFileEvent;
            _watcher.Deleted += OnWatchedFileEvent;

            var form = new Form();
            form.Size = new Size(1280, 720);
            form.MinimumSize = new Size(400, 300);
            form.Text = string.Format(Title, FileVersionInfo.GetVersionInfo(typeof(MainForm).Assembly.Location).ProductVersion);

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
            };

            _archiveInput = new TextBox
            {
                PlaceholderText = "Input map or campaign...",
            };

            _openCloseArchiveButton = new Button
            {
                Text = "Open archive",
                Enabled = false,
            };

            _diagnosticsDisplay = new RichTextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = RichTextBoxScrollBars.Both,
                WordWrap = false,
            };

            _progressBar = new TextProgressBar
            {
                Dock = DockStyle.Bottom,
                Style = ProgressBarStyle.Continuous,
                VisualMode = TextProgressBar.ProgressBarDisplayMode.CustomText,
                Visible = false,
            };

            _openArchiveWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false,
            };

            _openArchiveWorker.DoWork += OpenArchiveBackgroundWork;
            _openArchiveWorker.ProgressChanged += OpenArchiveProgressChanged;
            _openArchiveWorker.RunWorkerCompleted += OpenArchiveCompleted;

            _saveArchiveWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false,
            };

            _saveArchiveWorker.DoWork += SaveArchiveBackgroundWork;
            _saveArchiveWorker.ProgressChanged += SaveArchiveProgressChanged;
            _saveArchiveWorker.RunWorkerCompleted += SaveArchiveCompleted;

            _adaptAllButton = new Button
            {
                Text = "Adapt all",
            };

            _adaptAllButton.Size = _adaptAllButton.PreferredSize;
            _adaptAllButton.Enabled = false;

            _saveAsButton = new Button
            {
                Text = "Save As...",
            };

            _saveAsButton.Size = _saveAsButton.PreferredSize;
            _saveAsButton.Enabled = false;

            _targetPatchesComboBox = new ComboBox
            {
                Enabled = false,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120,
            };

            _targetPatchesComboBox.Items.AddRange(_appSettings.TargetPatches.OrderByDescending(targetPatch => targetPatch.Patch).Select(targetPatch => (object)targetPatch.Patch).ToArray());
            if (_targetPatchesComboBox.Items.Count == 1)
            {
                _targetPatchesComboBox.SelectedIndex = 0;
            }

            _targetPatchesComboBox.SelectedIndexChanged += (s, e) =>
            {
                _targetPatch = (GamePatch?)_targetPatchesComboBox.SelectedItem;
                _adaptAllButton.Enabled = _targetPatch.HasValue && _fileList.Items.Count > 0;
                _getHelpButton.Enabled = _targetPatch.HasValue;
            };

            _targetPatchesComboBox.FormattingEnabled = true;
            _targetPatchesComboBox.Format += (s, e) =>
            {
                if (e.ListItem is GamePatch gamePatch)
                {
                    e.Value = gamePatch.PrettyPrint();
                }
            };

            _getHelpButton = new Button
            {
                Text = "Get help",
            };

            _getHelpButton.Size = _getHelpButton.PreferredSize;
            _getHelpButton.Enabled = false;

            _getHelpButton.Click += (s, e) =>
            {
                if (_targetPatch.HasValue)
                {
                    _ = new GetHelpForm(_archiveInput.Text, GetTargetPatch(_targetPatch.Value)).ShowDialog();
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

                openFileDialog.Filter = GetMpqArchiveFileTypeFilter(true);

                var openFileDialogResult = openFileDialog.ShowDialog();
                if (openFileDialogResult == DialogResult.OK)
                {
                    _archiveInput.Text = openFileDialog.FileName;
                }
            };

            _fileList = new FileListView();

            var fileListContextMenu = new FileListContextMenuStrip(_fileList);

            _fileList.ContextMenuStrip = fileListContextMenu;

            fileListContextMenu.Adapt += OnClickAdaptSelected;
            fileListContextMenu.Edit += OnClickEditSelected;
            fileListContextMenu.Save += OnClickSaveSelected;
            fileListContextMenu.Diff += OnClickDiffSelected;
            fileListContextMenu.Undo += OnClickUndoChangesSelected;
            fileListContextMenu.Remove += OnClickRemoveSelected;

            fileListContextMenu.EnableClickEvents();

            _openCloseArchiveButton.Size = _openCloseArchiveButton.PreferredSize;
            _openCloseArchiveButton.Click += OnClickOpenCloseMap;

            _adaptAllButton.Click += (s, e) =>
            {
                _targetPatchesComboBox.Enabled = false;
                var parentsToUpdate = new HashSet<ItemTag>();
                for (var i = 0; i < _fileList.Items.Count; i++)
                {
                    var item = _fileList.Items[i];
                    var tag = item.GetTag();

                    var adapter = tag.Adapter;
                    if (adapter != null && tag.Status == MapFileStatus.Pending)
                    {
                        var context = new AdaptFileContext
                        {
                            FileName = tag.FileName,
                            Archive = tag.MpqArchive,
                            TargetPatch = GetTargetPatch(_targetPatch.Value),
                            OriginPatch = tag.GetOriginPatch(_originPatch.Value),
                        };

                        tag.CurrentStream.Position = 0;
                        var adaptResult = adapter.Run(tag.CurrentStream, context);
                        adaptResult.Diagnostics = context.GetDiagnostics();
                        tag.UpdateAdaptResult(adaptResult);

                        if (tag.Parent != null)
                        {
                            parentsToUpdate.Add(tag.Parent);
                        }
                    }
                }

                foreach (var parent in parentsToUpdate)
                {
                    parent.ListViewItem.Update();
                }

                UpdateDiagnosticsDisplay();
            };

            _fileList.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete)
                {
                    OnClickRemoveSelected(s, e);
                }
            };

            _fileList.SelectedIndexChanged += OnFileSelectionChanged;

            _fileList.ItemActivate += OnClickEditSelected;

            _saveAsButton.Click += (s, e) =>
            {
                var saveFileDialog = new SaveFileDialog
                {
                    OverwritePrompt = true,
                    CreatePrompt = false,
                    FileName = $"{Path.GetFileNameWithoutExtension(_archiveInput.Text)} (adapted){Path.GetExtension(_archiveInput.Text)}",
                };

                saveFileDialog.Filter = GetMpqArchiveFileTypeFilter(false);

                var saveFileDialogResult = saveFileDialog.ShowDialog();
                if (saveFileDialogResult == DialogResult.OK)
                {
                    SaveArchive(saveFileDialog.FileName);
                }
            };

            var flowLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                Width = 640,
            };

            var inputArchiveFlowLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
            };

            var buttonsFlowLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
            };

            var targetPatchLabel = new Label
            {
                Text = "Target patch",
                TextAlign = ContentAlignment.BottomRight,
            };

            inputArchiveFlowLayout.AddControls(_archiveInput, _archiveInputBrowseButton, _openCloseArchiveButton);
            buttonsFlowLayout.AddControls(_adaptAllButton, _saveAsButton, targetPatchLabel, _targetPatchesComboBox, _getHelpButton);
            flowLayout.AddControls(inputArchiveFlowLayout, buttonsFlowLayout);

            splitContainer.Panel1.AddControls(_diagnosticsDisplay, flowLayout);
            splitContainer.Panel2.AddControls(_fileList);
            form.AddControls(splitContainer, _progressBar);

            targetPatchLabel.Size = targetPatchLabel.PreferredSize;

            splitContainer.Panel1.SizeChanged += (s, e) =>
            {
                var width = splitContainer.Panel1.Width;
                _archiveInput.Width = (width > 360 ? 360 : width) - 10;

                inputArchiveFlowLayout.Size = inputArchiveFlowLayout.GetPreferredSize(new Size(width, 0));
                buttonsFlowLayout.Size = buttonsFlowLayout.GetPreferredSize(new Size(width, 0));
                flowLayout.Height
                    = inputArchiveFlowLayout.Height + inputArchiveFlowLayout.Margin.Vertical
                    + buttonsFlowLayout.Height + buttonsFlowLayout.Margin.Vertical;
            };

            splitContainer.SplitterDistance = 640 - splitContainer.SplitterWidth;
            splitContainer.Panel1MinSize = 200;

            form.FormClosing += (s, e) =>
            {
                _archive?.Dispose();
                _openArchiveWorker?.Dispose();
                _saveArchiveWorker?.Dispose();
                _fileSelectionChangedEventTimer?.Dispose();
            };

            form.ShowDialog();
        }

        private static void ReloadSettings()
        {
            _appSettings = new AppSettings
            {
                TargetPatches = _configuration.GetSection(nameof(AppSettings.TargetPatches)).GetChildren().Select(targetPatch => new TargetPatch
                {
                    Patch = Enum.Parse<GamePatch>(targetPatch.GetSection(nameof(TargetPatch.Patch)).Value),
                    GameDataPath = targetPatch.GetSection(nameof(TargetPatch.GameDataPath)).Value,
                }).ToList(),
            };
        }

        private static string GetMpqArchiveFileTypeFilter(bool isOpenFileDialog)
        {
            var filters = new List<string>
            {
                "Warcraft III archive|*.w3m;*.w3x;*.w3n",
                "Warcraft III map|*.w3m;*.w3x",
                "Warcraft III campaign|*.w3n",
            };

#if USE_KEY_CONTAINER
            if (isOpenFileDialog)
            {
                filters.Add("Zip archive|*.zip");
            }
#endif

            filters.Add("All files|*.*");

            return string.Join('|', filters);
        }

        private static void OnClickOpenCloseMap(object? sender, EventArgs e)
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

        private static void OnFileSelectionChanged(object? sender, EventArgs e)
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

        private static void OnFileSelectionEventTimerTick(object? sender, EventArgs e)
        {
            _fileSelectionChangedEventTimer.Enabled = false;

            UpdateDiagnosticsDisplay();
        }

        private static void OnClickEditSelected(object? sender, EventArgs e)
        {
            if (_fileList.TryGetSelectedItemTag(out var tag))
            {
                if (tag.Adapter is null || !tag.Adapter.IsTextFile)
                {
                    return;
                }

                var scriptEditForm = new ScriptEditForm(tag.AdaptResult?.Diagnostics ?? Array.Empty<Diagnostic>());

                tag.CurrentStream.Position = 0;
                using (var reader = new StreamReader(tag.CurrentStream, leaveOpen: true))
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
                    tag.ListViewItem.Update(AdaptResult.ModifiedByUser(memoryStream));

                    _diagnosticsDisplay.Text = string.Empty;
                }
            }
        }

        private static void OnClickSaveSelected(object? sender, EventArgs e)
        {
            if (_fileList.TryGetSelectedItemTag(out var tag))
            {
                if (tag.CurrentStream is null || tag.Children is not null)
                {
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    OverwritePrompt = true,
                    CreatePrompt = false,
                    FileName = Path.GetFileName(tag.FileName) ?? tag.Adapter?.DefaultFileName,
                };

                var extension = Path.GetExtension(saveFileDialog.FileName);
                if (string.IsNullOrEmpty(extension))
                {
                    saveFileDialog.Filter = "All files|*.*";
                }
                else
                {
                    var fileTypeDescription = tag.Adapter?.MapFileDescription ?? $"{extension.TrimStart('.').ToUpperInvariant()} file";

                    saveFileDialog.Filter = $"{fileTypeDescription}|*{extension}|All files|*.*";
                }

                var saveFileDialogResult = saveFileDialog.ShowDialog();
                if (saveFileDialogResult == DialogResult.OK)
                {
                    using var fileStream = File.Create(saveFileDialog.FileName);

                    tag.CurrentStream.Position = 0;
                    tag.CurrentStream.CopyTo(fileStream);
                }
            }
        }

        private static void OnClickDiffSelected(object? sender, EventArgs e)
        {
            if (_fileList.TryGetSelectedItemTag(out var tag))
            {
                if (tag.Adapter is null || tag.AdaptResult?.AdaptedFileStream is null)
                {
                    return;
                }

                string oldText, newText;
                if (tag.Adapter.IsTextFile)
                {
                    tag.OriginalFileStream.Position = 0;
                    tag.AdaptResult.AdaptedFileStream.Position = 0;

                    using var oldStreamReader = new StreamReader(tag.OriginalFileStream, leaveOpen: true);
                    using var newStreamReader = new StreamReader(tag.AdaptResult.AdaptedFileStream, leaveOpen: true);

                    oldText = oldStreamReader.ReadToEnd();
                    newText = newStreamReader.ReadToEnd();
                }
                else if (tag.Adapter.IsJsonSerializationSupported)
                {
                    tag.OriginalFileStream.Position = 0;
                    tag.AdaptResult.AdaptedFileStream.Position = 0;

                    oldText = tag.Adapter.GetJson(tag.OriginalFileStream, tag.GetOriginPatch(_originPatch.Value));
                    newText = tag.Adapter.GetJson(tag.AdaptResult.AdaptedFileStream, _targetPatch.Value);
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
        }

        private static void OnClickAdaptSelected(object? sender, EventArgs e)
        {
            _targetPatchesComboBox.Enabled = false;
            for (var i = 0; i < _fileList.SelectedIndices.Count; i++)
            {
                var index = _fileList.SelectedIndices[i];
                var item = _fileList.Items[index];
                var tag = item.GetTag();

                if (tag.Children != null)
                {
                    foreach (var child in tag.Children)
                    {
                        var adapter = child.Adapter;
                        if (adapter != null && child.Status == MapFileStatus.Pending)
                        {
                            var context = new AdaptFileContext
                            {
                                FileName = child.FileName,
                                Archive = child.MpqArchive,
                                TargetPatch = GetTargetPatch(_targetPatch.Value),
                                OriginPatch = child.GetOriginPatch(_originPatch.Value),
                            };

                            tag.CurrentStream.Position = 0;
                            var adaptResult = adapter.Run(child.CurrentStream, context);
                            adaptResult.Diagnostics = context.GetDiagnostics();
                            child.UpdateAdaptResult(adaptResult);
                        }
                    }

                    item.Update();
                }
                else
                {
                    var adapter = tag.Adapter;
                    if (adapter != null && tag.Status == MapFileStatus.Pending)
                    {
                        var context = new AdaptFileContext
                        {
                            FileName = tag.FileName,
                            Archive = tag.MpqArchive,
                            TargetPatch = GetTargetPatch(_targetPatch.Value),
                            OriginPatch = tag.GetOriginPatch(_originPatch.Value),
                        };

                        tag.CurrentStream.Position = 0;
                        var adaptResult = adapter.Run(tag.CurrentStream, context);
                        adaptResult.Diagnostics = context.GetDiagnostics();
                        tag.UpdateAdaptResult(adaptResult);

                        if (tag.Parent != null)
                        {
                            tag.Parent.ListViewItem.Update();
                        }
                    }
                }
            }

            UpdateDiagnosticsDisplay();
        }

        private static void OnClickUndoChangesSelected(object? sender, EventArgs e)
        {
            for (var i = 0; i < _fileList.SelectedItems.Count; i++)
            {
                var item = _fileList.SelectedItems[i];
                var tag = item.GetTag();

                if (tag.AdaptResult?.AdaptedFileStream is not null)
                {
                    tag.AdaptResult.Dispose();
                }
                else if (tag.Status != MapFileStatus.Removed)
                {
                    continue;
                }

                item.Update(null);
            }
        }

        private static void OnClickRemoveSelected(object? sender, EventArgs e)
        {
            for (var i = 0; i < _fileList.SelectedItems.Count; i++)
            {
                var item = _fileList.SelectedItems[i];
                var tag = item.GetTag();

                if (tag.Status == MapFileStatus.Removed)
                {
                    continue;
                }

                tag.AdaptResult?.Dispose();

                item.Update(MapFileStatus.Removed);
            }
        }

        private static void OnArchiveInputTextChanged(object? sender, EventArgs e)
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

        private static void OnWatchedFileEvent(object? sender, EventArgs e)
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

        private static void UpdateDiagnosticsDisplay()
        {
            if (_fileList.TryGetSelectedItemTag(out var tag))
            {
                _diagnosticsDisplay.Text = string.Empty;

                if (tag.AdaptResult?.Diagnostics != null)
                {
                    var anyDiagnosticWritten = false;
                    var originalColor = _diagnosticsDisplay.SelectionColor;

                    void WriteDiagnostics(DiagnosticSeverity severity, Color color, string prefix)
                    {
                        foreach (var grouping in tag.AdaptResult.Diagnostics.Where(d => d.Descriptor.Severity == severity).Select(d => d.Message).GroupBy(m => m, StringComparer.Ordinal))
                        {
                            if (anyDiagnosticWritten)
                            {
                                _diagnosticsDisplay.AppendText(System.Environment.NewLine);
                            }
                            else
                            {
                                anyDiagnosticWritten = true;
                            }

                            _diagnosticsDisplay.SelectionStart = _diagnosticsDisplay.TextLength;
                            _diagnosticsDisplay.SelectionLength = 0;
                            _diagnosticsDisplay.SelectionColor = color;
                            _diagnosticsDisplay.AppendText(prefix);
                            _diagnosticsDisplay.SelectionColor = originalColor;

                            var count = grouping.Count();
                            _diagnosticsDisplay.AppendText(count > 1 ? $"{grouping.Key} ({count})" : grouping.Key);
                        }
                    }

                    WriteDiagnostics(DiagnosticSeverity.Error, Color.Red, "[ERR] ");
                    WriteDiagnostics(DiagnosticSeverity.Warning, Color.Orange, "[WRN] ");
                    WriteDiagnostics(DiagnosticSeverity.Info, Color.Blue, "[INF] ");
                }
            }
            else
            {
                _diagnosticsDisplay.Text = $"{_fileList.SelectedItems.Count} files selected.";
            }
        }

        private static void OpenArchive()
        {
            var fileInfo = new FileInfo(_archiveInput.Text);
            if (fileInfo.Exists)
            {
                _archiveInput.Enabled = false;
                _archiveInputBrowseButton.Enabled = false;
                _openCloseArchiveButton.Enabled = false;
                _openCloseArchiveButton.Text = "Close archive";

                _progressBar.Value = 0;
                _progressBar.Maximum = 1;
                _progressBar.CustomText = string.Empty;
                _progressBar.Visible = true;

                _openArchiveWorker.RunWorkerAsync(fileInfo.FullName);
            }
        }

        private static void OpenArchiveBackgroundWork(object? sender, DoWorkEventArgs e)
        {
            var filePath = (string)e.Argument;

#if USE_KEY_CONTAINER
            if (string.Equals(Path.GetExtension(filePath), ".zip", StringComparison.OrdinalIgnoreCase))
            {
                using var zipFile = Ionic.Zip.ZipFile.Read(filePath);

                var encryptedAesParameters = zipFile.Entries.SingleOrDefault(e => string.Equals(e.FileName, "aes.enc", StringComparison.OrdinalIgnoreCase));
                if (encryptedAesParameters is null)
                {
                    var mapEntry = zipFile.Entries.Single(e => Path.GetExtension(e.FileName).StartsWith(".w3", StringComparison.OrdinalIgnoreCase));

                    var mapStream = new MemoryStream();
                    mapEntry.Extract(mapStream);
                    mapStream.Position = 0;

                    _archive = MpqArchive.Open(mapStream, true);
                }
                else
                {
                    using var encryptedAesStream = new MemoryStream();
                    encryptedAesParameters.Extract(encryptedAesStream);

                    var cspParameters = new CspParameters
                    {
                        KeyContainerName = "War3App.MapAdapter.RsaKeyContainer",
                    };

                    using var rsaProvider = new RSACryptoServiceProvider(cspParameters)
                    {
                        PersistKeyInCsp = true,
                    };

                    var aesParameters = rsaProvider.Decrypt(encryptedAesStream.ToArray(), RSAEncryptionPadding.Pkcs1);
                    var aesKey = new byte[32];
                    var aesIV = new byte[16];

                    Array.Copy(aesParameters, aesKey, aesKey.Length);
                    Array.Copy(aesParameters, aesKey.Length, aesIV, 0, aesIV.Length);

                    using var aes = Aes.Create();
                    aes.Padding = PaddingMode.PKCS7;

                    var encryptedMapEntry = zipFile.Entries.Single(e => string.Equals(Path.GetExtension(e.FileName), ".aes", StringComparison.OrdinalIgnoreCase));

                    using var encryptedMapStream = new MemoryStream();
                    encryptedMapEntry.Extract(encryptedMapStream);
                    encryptedMapStream.Position = 0;

                    using var aesDecryptor = aes.CreateDecryptor(aesKey, aesIV);
                    using var cryptoStream = new CryptoStream(encryptedMapStream, aesDecryptor, CryptoStreamMode.Read);

                    var mapStream = new MemoryStream();
                    cryptoStream.CopyTo(mapStream);
                    mapStream.Position = 0;

                    _archive = MpqArchive.Open(mapStream, true);
                }
            }
            else
#endif
            {
                _archive = MpqArchive.Open(filePath, true);
            }

            _archive.DiscoverFileNames();

            var mapsList = new HashSet<string>();
            if (_archive.IsCampaignArchive(out var campaignInfo))
            {
                for (var i = 0; i < campaignInfo.Maps.Count; i++)
                {
                    mapsList.Add(campaignInfo.Maps[i].MapFilePath);
                }
            }
            else
            {
                using var mpqStream = _archive.OpenFile(MapInfo.FileName);
                using var reader = new BinaryReader(mpqStream);
                _originPatch = reader.ReadMapInfo().GetOriginGamePatch();
            }

            var listViewItems = new List<ListViewItem>();
            var possibleOriginPatches = new HashSet<GamePatch>();
            var files = _archive.ToList();

            var progress = new OpenArchiveProgress();
            progress.Maximum = files.Count;

            var index = 0;

            foreach (var file in files)
            {
                if (mapsList.Contains(file.FileName))
                {
                    var mapName = file.FileName;

                    using var mapArchiveStream = _archive.OpenFile(mapName);
                    using var mapArchive = MpqArchive.Open(mapArchiveStream, true);
                    mapArchive.DiscoverFileNames();

                    var children = new List<ListViewItem>();
                    var mapFiles = mapArchive.ToList();

                    progress.Maximum += mapFiles.Count;

                    var parentIndex = index;

                    foreach (var mapFile in mapArchive)
                    {
                        var subItem = ListViewItemExtensions.Create(new ItemTag(mapArchive, mapFile, ++index, mapName));

                        subItem.IndentCount = 1;
                        children.Add(subItem);

                        _openArchiveWorker.ReportProgress(0, progress);
                    }

                    using (var mapInfoFileStream = mapArchive.OpenFile(MapInfo.FileName))
                    {
                        using var reader = new BinaryReader(mapInfoFileStream);
                        var mapArchiveOriginPatch = reader.ReadMapInfo().GetOriginGamePatch();

                        var mapArchiveItem = ListViewItemExtensions.Create(new ItemTag(_archive, file, parentIndex, children.ToArray(), mapArchiveOriginPatch));

                        listViewItems.Add(mapArchiveItem);

                        _openArchiveWorker.ReportProgress(0, progress);

                        if (mapArchiveOriginPatch.HasValue)
                        {
                            possibleOriginPatches.Add(mapArchiveOriginPatch.Value);
                        }
                    }

                    foreach (var child in children)
                    {
                        listViewItems.Add(child);
                    }
                }
                else
                {
                    var item = ListViewItemExtensions.Create(new ItemTag(_archive, file, index));

                    listViewItems.Add(item);

                    _openArchiveWorker.ReportProgress(0, progress);
                }

                index++;
            }

            if (_originPatch is null)
            {
                _originPatch = possibleOriginPatches.Count == 1 ? possibleOriginPatches.Single() : _latestPatch;
            }

            e.Result = listViewItems;
        }

        private static void OpenArchiveProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is OpenArchiveProgress openArchiveProgress)
            {
                _progressBar.Value++;
                _progressBar.Maximum = openArchiveProgress.Maximum;
                _progressBar.CustomText = $"{_progressBar.Value} / {_progressBar.Maximum}";
            }
        }

        private static void OpenArchiveCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error is not null)
            {
                throw new ApplicationException("Background task 'open archive' did not complete succesfully.", e.Error);
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

                _targetPatchesComboBox.Enabled = _targetPatchesComboBox.Items.Count > 1;

                _targetPatch = (GamePatch?)_targetPatchesComboBox.SelectedItem;
                _adaptAllButton.Enabled = _targetPatch.HasValue && _fileList.Items.Count > 0;
                _getHelpButton.Enabled = _targetPatch.HasValue;

                _openCloseArchiveButton.Enabled = true;
                _saveAsButton.Enabled = true;

                _progressBar.Visible = false;
            }
        }

        private static void CloseArchive()
        {
            _archive.Dispose();
            _archive = null;

            _archiveInput.Enabled = true;
            _archiveInputBrowseButton.Enabled = true;

            _openCloseArchiveButton.Text = "Open archive";
            _adaptAllButton.Enabled = false;
            _saveAsButton.Enabled = false;
            _getHelpButton.Enabled = false;

            _targetPatchesComboBox.Enabled = false;
            _originPatch = null;

            _fileList.Reset();

            _diagnosticsDisplay.Text = string.Empty;

            GC.Collect();
        }

        private static void SaveArchive(string fileName)
        {
            var itemCount = 0;
            for (var i = 0; i < _fileList.Items.Count; i++)
            {
                var tag = _fileList.Items[i].GetTag();
                if (tag.Status != MapFileStatus.Removed)
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

        private static void SaveArchiveBackgroundWork(object? sender, DoWorkEventArgs e)
        {
            var archiveBuilder = new MpqArchiveBuilder(_archive);

            var progress = new SaveArchiveProgress();
            progress.Saving = false;

            for (var i = 0; i < _fileList.Items.Count; i++)
            {
                var tag = _fileList.Items[i].GetTag();
                if (tag.Parent is not null)
                {
                    continue;
                }

                if (tag.Status == MapFileStatus.Removed)
                {
                    if (tag.TryGetHashedFileName(out var hashedFileName))
                    {
                        archiveBuilder.RemoveFile(hashedFileName);
                    }
                    else
                    {
                        archiveBuilder.RemoveFile(_archive, tag.MpqEntry);
                    }
                }
                else if (tag.Children is not null)
                {
                    if (tag.Children.All(child => child.Status == MapFileStatus.Removed))
                    {
                        throw new InvalidOperationException("Parent tag should have been removed since all child tags are removed, but was " + tag.Status);
                    }
                    else if (tag.Children.Any(child => child.IsModified || child.Status == MapFileStatus.Removed))
                    {
                        // Assume at most one nested archive (for campaign archives), so no recursion.
                        using var subArchive = MpqArchive.Open(_archive.OpenFile(tag.FileName));
                        foreach (var child in tag.Children)
                        {
                            if (child.FileName != null)
                            {
                                subArchive.AddFileName(child.FileName);
                            }
                        }

                        var subArchiveBuilder = new MpqArchiveBuilder(subArchive);
                        foreach (var child in tag.Children)
                        {
                            if (child.Status == MapFileStatus.Removed)
                            {
                                if (child.TryGetHashedFileName(out var hashedFileName))
                                {
                                    subArchiveBuilder.RemoveFile(hashedFileName);
                                }
                                else
                                {
                                    subArchiveBuilder.RemoveFile(subArchive, child.MpqEntry);
                                }
                            }
                            else if (child.TryGetModifiedMpqFile(out var subArchiveAdaptedFile))
                            {
                                subArchiveBuilder.AddFile(subArchiveAdaptedFile);

                                _saveArchiveWorker.ReportProgress(0, progress);
                            }
                            else
                            {
                                _saveArchiveWorker.ReportProgress(0, progress);
                            }
                        }

                        var adaptedSubArchiveStream = new MemoryStream();
                        subArchiveBuilder.SaveWithPreArchiveData(adaptedSubArchiveStream, true);

                        adaptedSubArchiveStream.Position = 0;
                        var adaptedFile = MpqFile.New(adaptedSubArchiveStream, tag.FileName, false);
                        adaptedFile.TargetFlags = tag.MpqEntry.Flags;
                        archiveBuilder.AddFile(adaptedFile);

                        _saveArchiveWorker.ReportProgress(0, progress);
                    }
                    else
                    {
                        _saveArchiveWorker.ReportProgress(tag.Children.Length, progress);
                    }
                }
                else if (tag.TryGetModifiedMpqFile(out var adaptedFile))
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

        private static void SaveArchiveProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is SaveArchiveProgress saveArchiveProgress)
            {
                if (saveArchiveProgress.Saving)
                {
                    _progressBar.CustomText = "Saving...";
                }
                else
                {
                    _progressBar.Value++;
                    _progressBar.Value += e.ProgressPercentage;
                    _progressBar.CustomText = $"{_progressBar.Value} / {_progressBar.Maximum}";
                }
            }
        }

        private static void SaveArchiveCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error is not null)
            {
                throw new ApplicationException("Background task 'save archive' did not complete succesfully.", e.Error);
            }
            else
            {
                _progressBar.Visible = false;
            }
        }

        private static TargetPatch GetTargetPatch(GamePatch patch)
        {
            return _appSettings.TargetPatches.First(targetPatch => targetPatch.Patch == patch);
        }
    }
}