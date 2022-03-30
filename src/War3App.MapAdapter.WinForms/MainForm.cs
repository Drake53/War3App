using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Microsoft.Extensions.Configuration;

using War3App.Common.WinForms;
using War3App.Common.WinForms.Extensions;
using War3App.MapAdapter.Info;
using War3App.MapAdapter.WinForms.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Info;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.WinForms
{
    internal static class MainForm
    {
        private const string Title = "Map Adapter v1.0.0";

        private const GamePatch LatestPatch = GamePatch.v1_32_10;

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

        private static ListView _fileList;
        private static FileListSorter _fileListSorter;

        private static Timer? _fileSelectionChangedEventTimer;

        private static ToolStripButton _editContextButton;
        private static ToolStripButton _adaptContextButton;
        private static ToolStripButton _removeContextButton;

        private static TextBox _diagnosticsDisplay;

        private static TextProgressBar _progressBar;
        private static BackgroundWorker _openArchiveWorker;
        private static BackgroundWorker _saveArchiveWorker;

        private static AppSettings _appSettings;

        [STAThread]
        private static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            _appSettings = new AppSettings
            {
                TargetPatches = configuration.GetSection(nameof(AppSettings.TargetPatches)).GetChildren().Select(targetPatch => new TargetPatch
                {
                    Patch = Enum.Parse<GamePatch>(targetPatch.GetSection(nameof(TargetPatch.Patch)).Value),
                    GameDataPath = targetPatch.GetSection(nameof(TargetPatch.GameDataPath)).Value,
                }).ToList(),
            };

            _watcher = new FileSystemWatcher();
            _watcher.Created += OnWatchedFileEvent;
            _watcher.Renamed += OnWatchedFileEvent;
            _watcher.Deleted += OnWatchedFileEvent;

            var form = new Form();
            form.Size = new Size(1280, 720);
            form.MinimumSize = new Size(400, 300);
            form.Text = Title;

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

            _diagnosticsDisplay = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
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
            };

            _targetPatchesComboBox.FormattingEnabled = true;
            _targetPatchesComboBox.Format += (s, e) =>
            {
                if (e.ListItem is GamePatch gamePatch)
                {
                    e.Value = gamePatch.ToString().Replace('_', '.');
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
                    "Warcraft III archive|*.w3m;*.w3x;*.w3n",
                    "Warcraft III map|*.w3m;*.w3x",
                    "Warcraft III campaign|*.w3n",
                    "All files|*.*",
                });
                var openFileDialogResult = openFileDialog.ShowDialog();
                if (openFileDialogResult == DialogResult.OK)
                {
                    _archiveInput.Text = openFileDialog.FileName;
                }
            };

            _fileList = new ListView
            {
                Dock = DockStyle.Fill,
            };

            _fileListSorter = new FileListSorter(_fileList);

            _fileList.ListViewItemSorter = _fileListSorter;
            _fileList.ColumnClick += _fileListSorter.Sort;

            _fileList.View = View.Details;
            _fileList.Columns.AddRange(new[]
            {
                new ColumnHeader { Text = "Status", Width = 102 },
                new ColumnHeader { Text = "FileName", Width = 300 },
                new ColumnHeader { Text = "FileType", Width = 130 },
                new ColumnHeader { Text = "Archive", Width = 87 },
            });

            _fileList.FullRowSelect = true;
            _fileList.MultiSelect = true;

            _fileList.HeaderStyle = ColumnHeaderStyle.Clickable;

            _fileList.SmallImageList = new ImageList();
            var statusColors = new Dictionary<MapFileStatus, Color>
            {
                { MapFileStatus.Adapted, Color.LimeGreen },
                { MapFileStatus.AdapterError, Color.Red },
                { MapFileStatus.Compatible, Color.ForestGreen },
                { MapFileStatus.ConfigError, Color.DarkSlateBlue },
                { MapFileStatus.Incompatible, Color.Yellow },
                { MapFileStatus.Locked, Color.OrangeRed },
                { MapFileStatus.Modified, Color.Blue },
                { MapFileStatus.ParseError, Color.Maroon },
                { MapFileStatus.Pending, Color.LightSkyBlue },
                { MapFileStatus.Removed, Color.DarkSlateGray },
                { MapFileStatus.Unadaptable, Color.IndianRed },
                { MapFileStatus.Unknown, Color.DarkViolet },
            };

            foreach (var status in Enum.GetValues(typeof(MapFileStatus)))
            {
                _fileList.SmallImageList.Images.Add(new Bitmap(16, 16).WithSolidColor(statusColors[(MapFileStatus)status]));
            }

            var fileListContextMenu = new ContextMenuStrip
            {
            };

            _editContextButton = new ToolStripButton("Edit");
            _editContextButton.Enabled = false;
            _editContextButton.Click += OnClickEditSelected;

            _adaptContextButton = new ToolStripButton("Adapt");
            _adaptContextButton.Enabled = false;
            _adaptContextButton.Click += OnClickAdaptSelected;

            _removeContextButton = new ToolStripButton("Remove");
            _removeContextButton.Enabled = false;
            _removeContextButton.Click += OnClickRemoveSelected;

            fileListContextMenu.Items.AddRange(new[]
            {
                _adaptContextButton,
                _editContextButton,
                _removeContextButton,
            });

            _fileList.ContextMenuStrip = fileListContextMenu;

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
                    if (adapter != null && (tag.Status == MapFileStatus.Pending || tag.Status == MapFileStatus.Modified))
                    {
                        tag.CurrentStream.Position = 0;
                        var adaptResult = adapter.AdaptFile(tag.CurrentStream, GetTargetPatch(_targetPatch.Value), tag.GetOriginPatch(_originPatch.Value));
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
                };

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
            buttonsFlowLayout.AddControls(_adaptAllButton, _saveAsButton, targetPatchLabel, _targetPatchesComboBox);
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
                    = inputArchiveFlowLayout.Margin.Top + inputArchiveFlowLayout.Height + inputArchiveFlowLayout.Margin.Bottom
                    + buttonsFlowLayout.Margin.Top + buttonsFlowLayout.Height + buttonsFlowLayout.Margin.Bottom;
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

        private static void OnFileSelectionChanged(object sender, EventArgs e)
        {
            if (_fileSelectionChangedEventTimer is null)
            {
                _fileSelectionChangedEventTimer = new Timer();
                _fileSelectionChangedEventTimer.Tick += OnFileSelectionEventTimerTick;
                _fileSelectionChangedEventTimer.Interval = 50;

                _editContextButton.Enabled = false;
                _adaptContextButton.Enabled = false;
                _removeContextButton.Enabled = false;
            }

            // Start/reset the timer
            _fileSelectionChangedEventTimer.Enabled = false;
            _fileSelectionChangedEventTimer.Enabled = true;
        }

        private static void OnFileSelectionEventTimerTick(object sender, EventArgs e)
        {
            if (_fileList.TryGetSelectedItemTag(out var tag))
            {
                _editContextButton.Enabled = tag.Adapter?.IsTextFile ?? false;
                _adaptContextButton.Enabled = _targetPatch.HasValue && (tag.Status == MapFileStatus.Pending || tag.Status == MapFileStatus.Modified);
                _removeContextButton.Enabled = tag.Status != MapFileStatus.Removed;
            }
            else
            {
                var tags = _fileList.GetSelectedItemTags();

                _editContextButton.Enabled = false;
                _adaptContextButton.Enabled = _targetPatch.HasValue && tags.Any(tag => tag.Status == MapFileStatus.Pending || tag.Status == MapFileStatus.Modified);
                _removeContextButton.Enabled = tags.Any(tag => tag.Status != MapFileStatus.Removed);
            }

            UpdateDiagnosticsDisplay();
        }

        private static void OnClickEditSelected(object sender, EventArgs e)
        {
            if (!_editContextButton.Enabled)
            {
                // Check since this method can also be invoked by ItemActivate event.
                return;
            }

            if (_fileList.TryGetSelectedItemTag(out var tag))
            {
                var scriptEditForm = new ScriptEditForm(tag.AdaptResult?.RegexDiagnostics ?? Array.Empty<RegexDiagnostic>());

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
                    tag.ListViewItem.Update(new AdaptResult
                    {
                        AdaptedFileStream = memoryStream,
                        Status = MapFileStatus.Modified,
                        Diagnostics = null,
                    });

                    _diagnosticsDisplay.Text = string.Empty;
                }
            }
        }

        private static void OnClickAdaptSelected(object sender, EventArgs e)
        {
            _adaptContextButton.Enabled = false;
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
                        if (adapter != null && (child.Status == MapFileStatus.Pending || child.Status == MapFileStatus.Modified))
                        {
                            tag.CurrentStream.Position = 0;
                            var adaptResult = adapter.AdaptFile(child.CurrentStream, GetTargetPatch(_targetPatch.Value), child.GetOriginPatch(_originPatch.Value));
                            child.UpdateAdaptResult(adaptResult);
                        }
                    }

                    item.Update();
                }
                else
                {
                    var adapter = tag.Adapter;
                    if (adapter != null && (tag.Status == MapFileStatus.Pending || tag.Status == MapFileStatus.Modified))
                    {
                        tag.CurrentStream.Position = 0;
                        var adaptResult = adapter.AdaptFile(tag.CurrentStream, GetTargetPatch(_targetPatch.Value), tag.GetOriginPatch(_originPatch.Value));
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

        private static void OnClickRemoveSelected(object sender, EventArgs e)
        {
            for (var i = 0; i < _fileList.SelectedItems.Count; i++)
            {
                var item = _fileList.SelectedItems[i];
                var tag = item.GetTag();

                if (tag.Status == MapFileStatus.Removed)
                {
                    continue;
                }

                tag.AdaptResult?.AdaptedFileStream?.Dispose();
                tag.AdaptResult = new AdaptResult
                {
                    Status = MapFileStatus.Removed,
                };

                item.Update();
            }

            _removeContextButton.Enabled = false;
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

        private static void UpdateDiagnosticsDisplay()
        {
            if (_fileList.TryGetSelectedItemTag(out var tag))
            {
                if (tag.AdaptResult?.Diagnostics != null)
                {
                    _diagnosticsDisplay.Lines = tag.AdaptResult.Diagnostics;
                }
                else
                {
                    _diagnosticsDisplay.Text = string.Empty;
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
            _archive = MpqArchive.Open((string)e.Argument, true);
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

                    foreach (var mapFile in mapArchive)
                    {
                        var subItem = ListViewItemExtensions.Create(new ItemTag(mapArchive, mapFile, mapName));

                        subItem.IndentCount = 1;
                        children.Add(subItem);

                        _openArchiveWorker.ReportProgress(0, progress);
                    }

                    using (var mapInfoFileStream = mapArchive.OpenFile(MapInfo.FileName))
                    {
                        using var reader = new BinaryReader(mapInfoFileStream);
                        var mapArchiveOriginPatch = reader.ReadMapInfo().GetOriginGamePatch();

                        var mapArchiveItem = ListViewItemExtensions.Create(new ItemTag(_archive, file, children.ToArray(), mapArchiveOriginPatch));

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
                    var item = ListViewItemExtensions.Create(new ItemTag(_archive, file));

                    listViewItems.Add(item);

                    _openArchiveWorker.ReportProgress(0, progress);
                }
            }

            if (_originPatch is null)
            {
                _originPatch = possibleOriginPatches.Count == 1 ? possibleOriginPatches.Single() : LatestPatch;
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
                throw e.Error;
            }
            else
            {
                _fileList.Items.AddRange(((List<ListViewItem>)e.Result).ToArray());

                _targetPatchesComboBox.Enabled = _targetPatchesComboBox.Items.Count > 1;

                _targetPatch = (GamePatch?)_targetPatchesComboBox.SelectedItem;
                _adaptAllButton.Enabled = _targetPatch.HasValue && _fileList.Items.Count > 0;

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

            _targetPatchesComboBox.Enabled = false;
            _targetPatchesComboBox.SelectedIndex = -1;
            _targetPatchesComboBox.Items.Clear();
            _originPatch = null;

            _fileListSorter.Reset();

            for (var i = 0; i < _fileList.Items.Count; i++)
            {
                var item = _fileList.Items[i].GetTag();
                item.OriginalFileStream?.Dispose();
                item.AdaptResult?.AdaptedFileStream?.Dispose();
            }

            _fileList.Items.Clear();

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
                throw e.Error;
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