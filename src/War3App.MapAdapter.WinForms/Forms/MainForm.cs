#define USE_KEY_CONTAINER

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Microsoft.Extensions.Configuration;

using War3App.Common.WinForms;
using War3App.Common.WinForms.Extensions;
using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.Extensions;
using War3App.MapAdapter.WinForms.Controls;
using War3App.MapAdapter.WinForms.Extensions;
using War3App.MapAdapter.WinForms.Helpers;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.WinForms.Forms
{
    [DesignerCategory("")]
    internal partial class MainForm : Form
    {
        private static MainForm _mainForm;

        private readonly GamePatch _latestPatch = Enum.GetValues<GamePatch>().Max();
        private readonly IConfiguration _configuration;
        private readonly BackgroundWorker _openArchiveWorker;
        private readonly BackgroundWorker _saveArchiveWorker;
        private readonly FileSystemWatcher _watcher;

        private readonly TextBox _archiveInput;
        private readonly Button _archiveInputBrowseButton;
        private readonly Button _openCloseArchiveButton;
        private readonly ComboBox _targetPatchesComboBox;
        private readonly Button _adaptAllButton;
        private readonly Button _saveAsButton;
        private readonly Button _getHelpButton;
        private readonly DiagnosticsDisplay _diagnosticsDisplay;
        private readonly FileListView _fileList;
        private readonly TextProgressBar _progressBar;

        private AppSettings _appSettings;
        private MpqArchive? _archive;
        private Timer? _fileSelectionChangedEventTimer;
        private bool _isTargetPatchFromZipArchive;
        private TargetPatch? _targetPatch;
        private GamePatch? _originPatch;

        public MainForm(IConfiguration configuration)
        {
            if (_mainForm is not null)
            {
                throw new InvalidOperationException("Cannot create multiple main forms.");
            }

            _mainForm = this;

            Size = new Size(1280, 720);
            MinimumSize = new Size(400, 300);
            Text = string.Format(TitleText.Main, typeof(MainForm).Assembly.GetVersionString());

            _configuration = configuration;
            _openArchiveWorker = CreateOpenArchiveWorker();
            _saveArchiveWorker = CreateSaveArchiveWorker();
            _watcher = CreateWatcher();

            _archiveInput = ControlFactory.TextBox(PlaceholderText.Archive);
            _archiveInputBrowseButton = ControlFactory.Button(ButtonText.Browse, enabled: true);
            _openCloseArchiveButton = ControlFactory.Button(ButtonText.Open);
            _targetPatchesComboBox = ControlFactory.DropDownList(width: 120);
            _adaptAllButton = ControlFactory.Button(ButtonText.AdaptAll);
            _saveAsButton = ControlFactory.Button(ButtonText.SaveAs);
            _getHelpButton = ControlFactory.Button(ButtonText.GetHelp);
            _diagnosticsDisplay = ControlFactory.DiagnosticsDisplay();
            _fileList = ControlFactory.FileListView();
            _progressBar = ControlFactory.TextProgressBar();

            ReloadSettings();

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

            _getHelpButton.Click += (s, e) =>
            {
                if (_targetPatch is not null)
                {
                    _ = new GetHelpForm(_archiveInput.Text, _targetPatch).ShowDialog();
                }
            };

            _archiveInput.TextChanged += OnArchiveInputTextChanged;

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

            var splitContainer = ControlFactory.SplitContainer();
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

        [MemberNotNull(nameof(_appSettings))]
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

        private FileSystemWatcher CreateWatcher()
        {
            var watcher = new FileSystemWatcher();

            watcher.Created += OnWatchedFileEvent;
            watcher.Renamed += OnWatchedFileEvent;
            watcher.Deleted += OnWatchedFileEvent;

            return watcher;
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
            _diagnosticsDisplay.Update(_fileList.SelectedItems);
        }

        private TargetPatch? GetTargetPatch(GamePatch? patch)
        {
            return patch.HasValue
                ? _appSettings.TargetPatches.FirstOrDefault(targetPatch => targetPatch.Patch == patch.Value)
                : null;
        }
    }
}