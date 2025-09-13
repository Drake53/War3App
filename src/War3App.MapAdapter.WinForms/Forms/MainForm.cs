#define USE_KEY_CONTAINER

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

using Microsoft.Extensions.Configuration;

using War3App.Common.WinForms;
using War3App.Common.WinForms.Extensions;
using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.Extensions;
using War3App.MapAdapter.Info;
using War3App.MapAdapter.WinForms.Controls;
using War3App.MapAdapter.WinForms.Extensions;
using War3App.MapAdapter.WinForms.Helpers;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Info;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.WinForms.Forms
{
    [DesignerCategory("")]
    internal class MainForm : Form
    {
        private readonly GamePatch _latestPatch = Enum.GetValues<GamePatch>().Max();

        private static MainForm _mainForm;

        private MpqArchive? _archive;
        private bool _isTargetPatchFromZipArchive;

        private readonly TextBox _archiveInput;
        private readonly Button _archiveInputBrowseButton;
        private readonly Button _openCloseArchiveButton;
        private readonly FileSystemWatcher _watcher;

        private readonly Button _adaptAllButton;
        private readonly Button _saveAsButton;
        private readonly ComboBox _targetPatchesComboBox;
        private TargetPatch? _targetPatch;
        private GamePatch? _originPatch;
        private readonly Button _getHelpButton;

        private readonly FileListView _fileList;

        private Timer? _fileSelectionChangedEventTimer;

        private readonly RichTextBox _diagnosticsDisplay;

        private readonly TextProgressBar _progressBar;
        private readonly BackgroundWorker _openArchiveWorker;
        private readonly BackgroundWorker _saveArchiveWorker;

        private readonly IConfiguration _configuration;
        private AppSettings _appSettings;

        public MainForm(IConfiguration configuration)
        {
            if (_mainForm is not null)
            {
                throw new InvalidOperationException("Cannot create multiple main forms.");
            }

            _mainForm = this;

            _configuration = configuration;

            ReloadSettings();

            _watcher = new FileSystemWatcher();
            _watcher.Created += OnWatchedFileEvent;
            _watcher.Renamed += OnWatchedFileEvent;
            _watcher.Deleted += OnWatchedFileEvent;

            Size = new Size(1280, 720);
            MinimumSize = new Size(400, 300);
            Text = string.Format(TitleText.Main, typeof(MainForm).Assembly.GetVersionString());

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
            };

            _archiveInput = ControlFactory.TextBox(PlaceholderText.Archive);
            _openCloseArchiveButton = ControlFactory.Button(ButtonText.Open);
            _diagnosticsDisplay = ControlFactory.RichTextBox();
            _progressBar = ControlFactory.TextProgressBar();

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

            _adaptAllButton = ControlFactory.Button(ButtonText.AdaptAll);
            _saveAsButton = ControlFactory.Button(ButtonText.SaveAs);
            _targetPatchesComboBox = ControlFactory.DropDownList(width: 120);

            _targetPatchesComboBox.Items.AddRange(_appSettings.TargetPatches.OrderByDescending(targetPatch => targetPatch.Patch).Select(targetPatch => (object)targetPatch.Patch).ToArray());
            if (_targetPatchesComboBox.Items.Count == 1)
            {
                _targetPatchesComboBox.SelectedIndex = 0;
            }

            _targetPatchesComboBox.SelectedIndexChanged += (s, e) =>
            {
                _targetPatch = GetTargetPatch((GamePatch?)_targetPatchesComboBox.SelectedItem);
                _adaptAllButton.Enabled = CanAdapt && _fileList.Items.Count > 0;
                _getHelpButton.Enabled = _targetPatch is not null;
            };

            _targetPatchesComboBox.FormattingEnabled = true;
            _targetPatchesComboBox.Format += (s, e) =>
            {
                if (e.ListItem is GamePatch gamePatch)
                {
                    e.Value = gamePatch.PrettyPrint();
                }
            };

            _getHelpButton = ControlFactory.Button(ButtonText.GetHelp);

            _getHelpButton.Click += (s, e) =>
            {
                if (_targetPatch is not null)
                {
                    _ = new GetHelpForm(_archiveInput.Text, _targetPatch).ShowDialog();
                }
            };

            _archiveInput.TextChanged += OnArchiveInputTextChanged;

            _archiveInputBrowseButton = ControlFactory.Button(ButtonText.Browse, enabled: true);

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

            fileListContextMenu.RegisterClickEvents();

            _openCloseArchiveButton.Click += OnClickOpenCloseMap;

            _adaptAllButton.Click += (s, e) =>
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
                    FileName = $"{Path.GetFileNameWithoutExtension(_archiveInput.Text)}{MiscStrings.AdaptedFileTag}{Path.GetExtension(_archiveInput.Text)}",
                };

                saveFileDialog.Filter = GetMpqArchiveFileTypeFilter(false);

                var saveFileDialogResult = saveFileDialog.ShowDialog();
                if (saveFileDialogResult == DialogResult.OK)
                {
                    SaveArchive(saveFileDialog.FileName);
                }
            };

            var targetPatchLabel = ControlFactory.Label(LabelText.TargetPatch);

            var inputArchiveFlowLayout = ControlFactory.HorizontalLayoutPanel(_archiveInput, _archiveInputBrowseButton, _openCloseArchiveButton);
            var buttonsFlowLayout = ControlFactory.HorizontalLayoutPanel(_adaptAllButton, _saveAsButton, targetPatchLabel, _targetPatchesComboBox, _getHelpButton);
            var flowLayout = ControlFactory.VerticalLayoutPanel(width: 640, inputArchiveFlowLayout, buttonsFlowLayout);

            splitContainer.Panel1.AddControls(_diagnosticsDisplay, flowLayout);
            splitContainer.Panel2.AddControls(_fileList);
            this.AddControls(splitContainer, _progressBar);

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

            FormClosing += (s, e) =>
            {
                _archive?.Dispose();
                _openArchiveWorker?.Dispose();
                _saveArchiveWorker?.Dispose();
                _fileSelectionChangedEventTimer?.Dispose();
            };
        }

        internal static MainForm Instance => _mainForm;

        [MemberNotNullWhen(true, nameof(_targetPatch))]
        internal bool CanAdapt => _targetPatch is not null;

        private void ReloadSettings()
        {
            _appSettings = new AppSettings
            {
                TargetPatches = _configuration.GetSection(nameof(AppSettings.TargetPatches)).GetChildren().Select(targetPatch => new TargetPatch
                {
                    Patch = Enum.Parse<GamePatch>(targetPatch.GetSection(nameof(TargetPatch.Patch)).Value),
                    GameDataPath = targetPatch.GetSection(nameof(TargetPatch.GameDataPath)).Value,
                    GameDataContainerType = ContainerType.Directory,
                }).ToList(),
            };
        }

        private static string GetMpqArchiveFileTypeFilter(bool isOpenFileDialog)
        {
            var filters = new List<string>
            {
                FilterStrings.ArchiveFile,
                FilterStrings.MapFile,
                FilterStrings.CampaignFile,
            };

#if USE_KEY_CONTAINER
            if (isOpenFileDialog)
            {
                filters.Add(FilterStrings.ZipArchive);
            }
#endif

            filters.Add(FilterStrings.AllFiles);

            return string.Join(FilterStrings.Separator, filters);
        }

        private void OnClickOpenCloseMap(object? sender, EventArgs e)
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

        private void OnClickEditSelected(object? sender, EventArgs e)
        {
            if (_fileList.TryGetSelectedMapFile(out var mapFile))
            {
                if (mapFile.Adapter is null || !mapFile.Adapter.IsTextFile)
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
        }

        private void OnClickSaveSelected(object? sender, EventArgs e)
        {
            if (_fileList.TryGetSelectedMapFile(out var mapFile))
            {
                if (mapFile.CurrentStream is null || mapFile.Children is not null)
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

                    saveFileDialog.Filter = $"{fileTypeDescription}|*{extension}{FilterStrings.Separator}{FilterStrings.AllFiles}";
                }

                var saveFileDialogResult = saveFileDialog.ShowDialog();
                if (saveFileDialogResult == DialogResult.OK)
                {
                    using var fileStream = File.Create(saveFileDialog.FileName);

                    mapFile.CurrentStream.Position = 0;
                    mapFile.CurrentStream.CopyTo(fileStream);
                }
            }
        }

        private void OnClickDiffSelected(object? sender, EventArgs e)
        {
            if (_fileList.TryGetSelectedMapFile(out var mapFile))
            {
                if (mapFile.Adapter is null || mapFile.AdaptResult?.AdaptedFileStream is null)
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

        private void OnWatchedFileEvent(object? sender, EventArgs e)
        {
            SetOpenArchiveButtonEnabled(File.Exists(_archiveInput.Text));
        }

        private void SetOpenArchiveButtonEnabled(bool enabled)
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

        private void UpdateDiagnosticsDisplay()
        {
            if (_fileList.SelectedItems.Count == 0)
            {
                _diagnosticsDisplay.Text = PlaceholderText.Diagnostics;
                return;
            }

            var isFirstItem = true;

            _diagnosticsDisplay.Text = string.Empty;

            foreach (ListViewItem item in _fileList.SelectedItems)
            {
                var mapFile = item.GetMapFile();

                if (!isFirstItem)
                {
                    _diagnosticsDisplay.WriteLine();
                    _diagnosticsDisplay.WriteLine();
                }

                isFirstItem = false;

                _diagnosticsDisplay.Write($"// {mapFile.CurrentFileName}", Color.Green);

                if (mapFile.AdaptResult?.Diagnostics is null ||
                    mapFile.AdaptResult.Diagnostics.Length == 0)
                {
                    _diagnosticsDisplay.WriteLine();
                    _diagnosticsDisplay.Write(DiagnosticText.None, Color.Gray);

                    continue;
                }

                void WriteDiagnostics(DiagnosticSeverity severity, Color color, string prefix)
                {
                    foreach (var grouping in mapFile.AdaptResult.Diagnostics.Where(d => d.Descriptor.Severity == severity).Select(d => d.Message).GroupBy(m => m, StringComparer.Ordinal))
                    {
                        _diagnosticsDisplay.WriteLine();
                        _diagnosticsDisplay.Write(prefix, color);

                        var count = grouping.Count();
                        _diagnosticsDisplay.AppendText(count > 1 ? $"{grouping.Key} ({count}x)" : grouping.Key);
                    }
                }

                WriteDiagnostics(DiagnosticSeverity.Error, Color.Red, DiagnosticText.Error);
                WriteDiagnostics(DiagnosticSeverity.Warning, Color.Orange, DiagnosticText.Warning);
                WriteDiagnostics(DiagnosticSeverity.Info, Color.Blue, DiagnosticText.Info);
            }
        }

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

        private void OpenArchiveBackgroundWork(object? sender, DoWorkEventArgs e)
        {
            var filePath = (string)e.Argument;

#if USE_KEY_CONTAINER
            if (string.Equals(Path.GetExtension(filePath), FileExtension.Zip, StringComparison.OrdinalIgnoreCase))
            {
                using var stream = File.OpenRead(filePath);
                using var zipFile = new ZipArchive(stream, ZipArchiveMode.Read);

                var encryptedAesParameters = zipFile.Entries.SingleOrDefault(e => string.Equals(e.FullName, FileName.AesParameters, StringComparison.OrdinalIgnoreCase));
                if (encryptedAesParameters is null)
                {
                    var mapEntry = zipFile.Entries.Single(e => Path.GetExtension(e.FullName).StartsWith(".w3", StringComparison.OrdinalIgnoreCase));

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
                        KeyContainerName = MiscStrings.EncryptionKeyContainerName,
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

                    var encryptedMapEntry = zipFile.Entries.Single(e => string.Equals(Path.GetExtension(e.FullName), FileExtension.Aes, StringComparison.OrdinalIgnoreCase));

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

                var gamePatchEntry = zipFile.Entries.SingleOrDefault(e => string.Equals(e.FullName, FileName.TargetPatch, StringComparison.OrdinalIgnoreCase));
                using var gamePatchStream = gamePatchEntry.Open();

                _targetPatch = new TargetPatch
                {
                    Patch = Enum.Parse<GamePatch>(gamePatchStream.ReadAllText()),
                    GameDataPath = filePath,
                    GameDataContainerType = ContainerType.ZipArchive,
                };

                _isTargetPatchFromZipArchive = true;
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
                        var subItem = ListViewItemFactory.Create(new MapFile(mapArchive, mapFile, ++index, mapName), _fileList);

                        subItem.IndentCount = 1;
                        children.Add(subItem);

                        _openArchiveWorker.ReportProgress(0, progress);
                    }

                    using (var mapInfoFileStream = mapArchive.OpenFile(MapInfo.FileName))
                    {
                        using var reader = new BinaryReader(mapInfoFileStream);
                        var mapArchiveOriginPatch = reader.ReadMapInfo().GetOriginGamePatch();

                        var childMapFiles = children.Select(child => child.GetMapFile()).ToArray();
                        var mapArchiveItem = ListViewItemFactory.Create(new MapFile(_archive, file, parentIndex, childMapFiles, mapArchiveOriginPatch), _fileList);

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
                    var item = ListViewItemFactory.Create(new MapFile(_archive, file, index), _fileList);

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
                        using var subArchive = MpqArchive.Open(_archive.OpenFile(mapFile.OriginalFileName));
                        foreach (var child in mapFile.Children)
                        {
                            if (child.OriginalFileName is not null)
                            {
                                subArchive.AddFileName(child.OriginalFileName);
                            }
                        }

                        var subArchiveBuilder = new MpqArchiveBuilder(subArchive);
                        foreach (var child in mapFile.Children)
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
                        var adaptedFile = MpqFile.New(adaptedSubArchiveStream, mapFile.CurrentFileName, false);
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

        private TargetPatch? GetTargetPatch(GamePatch? patch)
        {
            return patch.HasValue
                ? _appSettings.TargetPatches.FirstOrDefault(targetPatch => targetPatch.Patch == patch.Value)
                : null;
        }
    }
}