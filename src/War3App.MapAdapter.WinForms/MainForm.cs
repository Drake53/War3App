using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using War3App.MapAdapter.Info;
using War3App.MapAdapter.WinForms.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Info;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.WinForms
{
    internal static class MainForm
    {
        private const string Title = "Map Adapter v0.5.0";

        private const GamePatch LatestPatch = GamePatch.v1_32_7;

        private static MpqArchive _archive;

        private static TextBox _archiveInput;
        private static Button _archiveInputBrowseButton;
        private static Button _openCloseArchiveButton;

        private static Button _adaptAllButton;
        private static Button _saveAsButton;
        private static ComboBox _targetPatchesComboBox;
        private static GamePatch? _targetPatch;
        private static GamePatch? _originPatch;

        private static ListView _fileList;
        private static ToolStripButton _editContextButton;
        private static ToolStripButton _adaptContextButton;
        private static ToolStripButton _removeContextButton;

        private static TextBox _diagnosticsDisplay;

        [STAThread]
        private static void Main(string[] args)
        {
            var form = new Form();
            form.Width = 1280;
            form.Height = 720;
            form.Text = Title;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;

            _archiveInput = new TextBox
            {
                Width = 350,
                Height = 20,
                PlaceholderText = "Input map or campaign...",
            };

            _openCloseArchiveButton = new Button
            {
                Text = "Open archive",
                Enabled = false,
            };

            _diagnosticsDisplay = new TextBox
            {
                Height = 600,
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Bottom,
                ScrollBars = ScrollBars.Vertical,
            };

            _adaptAllButton = new Button
            {
                Location = new Point(505, 45),
                Text = "Adapt all",
            };

            _adaptAllButton.Size = _adaptAllButton.PreferredSize;
            _adaptAllButton.Enabled = false;

            _saveAsButton = new Button
            {
                Location = new Point(505, 165),
                Text = "Save As...",
            };

            _saveAsButton.Size = _saveAsButton.PreferredSize;
            _saveAsButton.Enabled = false;

            _targetPatchesComboBox = new ComboBox
            {
                Enabled = false,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120,
                Dock = DockStyle.Top,
            };

            _targetPatchesComboBox.SelectedIndexChanged += (s, e) =>
            {
                _targetPatch = (GamePatch?)_targetPatchesComboBox.SelectedItem;
                _adaptAllButton.Enabled = _targetPatch.HasValue && _fileList.Items.Count > 0;
            };

            _archiveInput.TextChanged += (s, e) =>
            {
                TrySetOpenArchiveButtonEnabled();
            };

            _archiveInputBrowseButton = new Button
            {
                Location = new Point(420, 5),
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
                Dock = DockStyle.Right,
                Width = 640,
            };

            _fileList.View = View.Details;
            _fileList.Columns.AddRange(new[]
            {
                new ColumnHeader { Text = "Status", Width = 102 },
                new ColumnHeader { Text = "FileName", Width = 300 },
                new ColumnHeader { Text = "FileType", Width = 130 },
                new ColumnHeader { Text = "Archive", Width = 87 },
            });

            _fileList.FullRowSelect = true;
            _fileList.MultiSelect = false;

            _fileList.HeaderStyle = ColumnHeaderStyle.Clickable;

            _fileList.SmallImageList = new ImageList();
            var statusColors = new Dictionary<MapFileStatus, Color>
            {
                { MapFileStatus.Adapted, Color.LimeGreen },
                { MapFileStatus.AdapterError, Color.Red },
                { MapFileStatus.Compatible, Color.ForestGreen },
                { MapFileStatus.Incompatible, Color.Yellow },
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
                        var adaptResult = adapter.AdaptFile(tag.CurrentStream, _targetPatch.Value, tag.GetOriginPatch(_originPatch.Value));
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

            _fileList.SelectedIndexChanged += (s, e) =>
            {
                if (_fileList.TryGetSelectedItemTag(out var tag))
                {
                    _editContextButton.Enabled = tag.Adapter?.IsTextFile ?? false;
                    _adaptContextButton.Enabled = _targetPatch.HasValue && (tag.Status == MapFileStatus.Pending || tag.Status == MapFileStatus.Modified);
                    _removeContextButton.Enabled = tag.Status != MapFileStatus.Removed;
                }
                else
                {
                    _editContextButton.Enabled = false;
                    _adaptContextButton.Enabled = false;
                    _removeContextButton.Enabled = false;
                }

                UpdateDiagnosticsDisplay();
            };

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
                Dock = DockStyle.Fill,
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
            form.AddControls(_diagnosticsDisplay, _fileList, flowLayout);

            targetPatchLabel.Size = targetPatchLabel.PreferredSize;
            buttonsFlowLayout.Size = buttonsFlowLayout.PreferredSize;

            inputArchiveFlowLayout.Size = inputArchiveFlowLayout.PreferredSize;
            flowLayout.Size = flowLayout.PreferredSize;

            form.FormClosing += (s, e) =>
            {
                _archive?.Dispose();
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
                            var adaptResult = adapter.AdaptFile(child.CurrentStream, _targetPatch.Value, child.GetOriginPatch(_originPatch.Value));
                            child.UpdateAdaptResult(adaptResult);
                        }
                    }

                    item.Update();
                }
                else
                {
                    var adapter = tag.Adapter;
                    if (adapter != null)
                    {
                        var adaptResult = adapter.AdaptFile(tag.CurrentStream, _targetPatch.Value, tag.GetOriginPatch(_originPatch.Value));
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

                tag.AdaptResult?.AdaptedFileStream?.Dispose();
                tag.AdaptResult = new AdaptResult
                {
                    Status = MapFileStatus.Removed,
                };

                item.Update();
            }

            _removeContextButton.Enabled = false;
        }

        private static void TrySetOpenArchiveButtonEnabled()
        {
            _openCloseArchiveButton.Enabled = string.IsNullOrWhiteSpace(_archiveInput.Text) ? false : File.Exists(_archiveInput.Text);
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
                _diagnosticsDisplay.Text = string.Empty;
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
                _archive.AddFileNames();

                var mapsList = new HashSet<string>();
                if (_archive.IsCampaignArchive(out var campaignInfo))
                {
                    for (var i = 0; i < campaignInfo.MapCount; i++)
                    {
                        mapsList.Add(campaignInfo.GetMap(i).MapFilePath);
                    }
                }
                else
                {
                    _originPatch = MapInfo.Parse(_archive.OpenFile(MapInfo.FileName)).GetOriginGamePatch();
                }

                var possibleOriginPatches = new HashSet<GamePatch>();
                foreach (var file in _archive)
                {
                    if (mapsList.Contains(file.Filename))
                    {
                        var map = file.Filename;

                        using var mapArchiveStream = _archive.OpenFile(map);
                        using var mapArchive = MpqArchive.Open(mapArchiveStream, true);
                        mapArchive.AddFileNames();

                        var children = new List<ListViewItem>();
                        foreach (var mapFile in mapArchive)
                        {
                            var subItem = ListViewItemExtensions.Create(new ItemTag(mapArchive, mapFile, map));
                            subItem.IndentCount = 1;
                            children.Add(subItem);
                        }

                        using (var mapInfoFileStream = mapArchive.OpenFile(MapInfo.FileName))
                        {
                            var mapArchiveOriginPatch = MapInfo.Parse(mapInfoFileStream).GetOriginGamePatch();
                            var mapArchiveItem = ListViewItemExtensions.Create(new ItemTag(_archive, file, children.ToArray(), mapArchiveOriginPatch));

                            _fileList.Items.Add(mapArchiveItem);
                            if (mapArchiveOriginPatch.HasValue)
                            {
                                possibleOriginPatches.Add(mapArchiveOriginPatch.Value);
                            }
                        }

                        foreach (var child in children)
                        {
                            _fileList.Items.Add(child);
                        }
                    }
                    else
                    {
                        var item = ListViewItemExtensions.Create(new ItemTag(_archive, file));

                        _fileList.Items.Add(item);
                    }
                }

                if (_originPatch is null && possibleOriginPatches.Count == 1)
                {
                    _originPatch = possibleOriginPatches.Single();
                }

                var targetPatches = new HashSet<object>(new object[] { GamePatch.v1_29_0, GamePatch.v1_31_0, LatestPatch, });
                if (_originPatch is null)
                {
                    _originPatch = LatestPatch;
                }
                //else if (_originPatch.Value <= GamePatch.v1_29_2)
                //{
                //    targetPatches.Remove(GamePatch.v1_29_0);
                //}
                //else if (_originPatch.Value <= GamePatch.v1_31_1)
                //{
                //    targetPatches.Remove(GamePatch.v1_31_0);
                //}
                //else
                //{
                //    targetPatches.Remove(LatestPatch);
                //}

                // TODO: Add object data for latest patch to prevent adapter errors.
                targetPatches.Remove(LatestPatch);

                _targetPatchesComboBox.Items.AddRange(targetPatches.ToArray());
                _targetPatchesComboBox.Enabled = true;
                if (_targetPatchesComboBox.Items.Count == 1)
                {
                    _targetPatchesComboBox.SelectedIndex = 0;
                }
            }
        }

        private static void CloseArchive()
        {
            _archive.Dispose();
            _archive = null;

            _archiveInput.Enabled = true;
            _archiveInputBrowseButton.Enabled = true;
            TrySetOpenArchiveButtonEnabled();

            _openCloseArchiveButton.Text = "Open archive";
            _adaptAllButton.Enabled = false;
            _saveAsButton.Enabled = false;

            _targetPatchesComboBox.Enabled = false;
            _targetPatchesComboBox.SelectedIndex = -1;
            _targetPatchesComboBox.Items.Clear();
            _originPatch = null;

            for (var i = 0; i < _fileList.Items.Count; i++)
            {
                _fileList.Items[i].GetTag().AdaptResult?.AdaptedFileStream?.Dispose();
            }

            _fileList.Items.Clear();
        }

        private static void SaveArchive(string fileName)
        {
            var originalFiles = _archive.GetMpqFiles();
            var adaptedFiles = new List<MpqFile>();
            var removedFiles = new HashSet<ulong>();

            for (var i = 0; i < _fileList.Items.Count; i++)
            {
                var tag = _fileList.Items[i].GetTag();
                if (tag.Parent != null)
                {
                    continue;
                }

                if (tag.Status == MapFileStatus.Removed)
                {
                    removedFiles.Add(tag.GetHashedFileName());
                }
                else if (tag.Children != null)
                {
                    if (tag.Children.All(child => child.Status == MapFileStatus.Removed))
                    {
                        removedFiles.Add(tag.GetHashedFileName());
                    }
                    else if (tag.Children.Any(child => child.IsModified || child.Status == MapFileStatus.Removed))
                    {
                        // Assume at most one nested archive (for campaign archives), so no recursion.
                        using var subArchive = MpqArchive.Open(_archive.OpenFile(tag.FileName));
                        var subArchiveAdaptedFiles = new List<MpqFile>();
                        var subArchiveRemovedFiles = new HashSet<ulong>();

                        foreach (var child in tag.Children)
                        {
                            if (child.FileName != null)
                            {
                                subArchive.AddFilename(child.FileName);
                            }

                            if (child.Status == MapFileStatus.Removed)
                            {
                                subArchiveRemovedFiles.Add(child.GetHashedFileName());
                            }
                            else if (child.TryGetModifiedMpqFile(out var subArchiveAdaptedFile))
                            {
                                subArchiveAdaptedFiles.Add(subArchiveAdaptedFile);
                            }
                        }

                        var adaptedSubArchiveStream = new MemoryStream();
                        // if (_targetPatch.Value < GamePatch.v1_31_0)
                        {
                            MapInfo.Parse(subArchive.OpenFile(MapInfo.FileName)).WriteArchiveHeaderToStream(adaptedSubArchiveStream);
                        }

                        var subArchiveOriginalFiles = subArchive.GetMpqFiles();
                        MpqArchive.Create(adaptedSubArchiveStream, GetCreateArchiveMpqFiles(subArchiveOriginalFiles, subArchiveAdaptedFiles, subArchiveRemovedFiles).ToArray());

                        adaptedSubArchiveStream.Position = 0;
                        var adaptedFile = MpqFile.New(adaptedSubArchiveStream, tag.FileName, true);
                        adaptedFile.TargetFlags = tag.MpqEntry.Flags;
                        adaptedFiles.Add(adaptedFile);
                    }
                }
                else if (tag.TryGetModifiedMpqFile(out var adaptedFile))
                {
                    adaptedFiles.Add(adaptedFile);
                }
            }

            using (var fileStream = File.Create(fileName))
            {
                // if (_targetPatch.Value < GamePatch.v1_31_0)
                {
                    if (_archive.IsCampaignArchive(out var campaignInfo))
                    {
                        campaignInfo.WriteArchiveHeaderToStream(fileStream);
                    }
                    else
                    {
                        MapInfo.Parse(_archive.OpenFile(MapInfo.FileName)).WriteArchiveHeaderToStream(fileStream);
                    }
                }

                MpqArchive.Create(fileStream, GetCreateArchiveMpqFiles(originalFiles, adaptedFiles, removedFiles).ToArray()).Dispose();
            }
        }

        private static IEnumerable<MpqFile> GetCreateArchiveMpqFiles(IEnumerable<MpqFile> originalFiles, IEnumerable<MpqFile> modifiedFiles, IEnumerable<ulong> removedFiles)
        {
            return modifiedFiles.Concat(originalFiles.Where(originalFile =>
                !removedFiles.Contains(originalFile.Name) &&
                !modifiedFiles.Where(modifiedFile => modifiedFile.IsSameAs(originalFile)).Any()));
        }
    }
}