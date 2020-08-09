using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using War3App.MapAdapter.WinForms.Extensions;

using War3Net.Build.Common;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.WinForms
{
    internal static class MainForm
    {
        private const GamePatch TargetPatch = GamePatch.v1_31_0;

        private static MpqArchive _archive;

        private static TextBox _archiveInput;
        private static Button _archiveInputBrowseButton;
        private static Button _openCloseArchiveButton;

        private static Button _adaptAllButton;
        private static Button _saveAsButton;

        private static ListView _fileList;
        private static ToolStripButton _adaptContextButton;
        private static ToolStripButton _removeContextButton;

        private static TextBox _diagnosticsDisplay;

        [STAThread]
        private static void Main(string[] args)
        {
            var form = new Form();
            form.Width = 1280;
            form.Height = 720;
            form.Text = "Map Adapter";
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
                new ColumnHeader { Text = "Status", Width = 100 },
                new ColumnHeader { Text = "FileName", Width = 296 },
                new ColumnHeader { Text = "Archive", Width = 100 },
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

            _adaptContextButton = new ToolStripButton("Adapt");
            _adaptContextButton.Enabled = false;
            _adaptContextButton.Click += OnClickAdaptSelected;
            fileListContextMenu.Items.Add(_adaptContextButton);

            _removeContextButton = new ToolStripButton("Remove");
            _removeContextButton.Enabled = false;
            _removeContextButton.Click += OnClickRemoveSelected;
            fileListContextMenu.Items.Add(_removeContextButton);

            _fileList.ContextMenuStrip = fileListContextMenu;

            _openCloseArchiveButton.Size = _openCloseArchiveButton.PreferredSize;
            _openCloseArchiveButton.Click += OnClickOpenCloseMap;

            _adaptAllButton.Click += (s, e) =>
            {
                var parentsToUpdate = new HashSet<ItemTag>();
                for (var i = 0; i < _fileList.Items.Count; i++)
                {
                    var item = _fileList.Items[i];
                    var tag = item.GetTag();

                    var adapter = tag.Adapter;
                    if (adapter != null && (tag.Status == MapFileStatus.Pending || tag.Status == MapFileStatus.Modified))
                    {
                        var adaptResult = adapter.AdaptFile(tag.CurrentStream, TargetPatch);
                        tag.UpdateAdaptResult(adaptResult);

                        if (adaptResult.Status == MapFileStatus.Adapted)
                        {
                            _saveAsButton.Enabled = true;
                        }

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
                if (_fileList.SelectedItems.Count == 1)
                {
                    var tag = _fileList.SelectedItems[0].GetTag();
                    _adaptContextButton.Enabled = (tag.Status == MapFileStatus.Pending || tag.Status == MapFileStatus.Modified);
                    _removeContextButton.Enabled = tag.Status != MapFileStatus.Removed;
                }
                else
                {
                    _adaptContextButton.Enabled = false;
                    _removeContextButton.Enabled = false;
                }

                UpdateDiagnosticsDisplay();
            };

            _fileList.ItemActivate += (s, e) =>
            {
                if (_fileList.SelectedItems.Count == 1)
                {
                    var tag = _fileList.SelectedItems[0].GetTag();
                    if (tag.Status == MapFileStatus.Incompatible)
                    {
                        var scriptEditForm = new ScriptEditForm(tag.AdaptResult.RegexDiagnostics);

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
            };

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

            inputArchiveFlowLayout.AddControls(_archiveInput, _archiveInputBrowseButton, _openCloseArchiveButton);
            buttonsFlowLayout.AddControls(_adaptAllButton, _saveAsButton);
            flowLayout.AddControls(inputArchiveFlowLayout, buttonsFlowLayout);
            form.AddControls(_diagnosticsDisplay, _fileList, flowLayout);

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

        private static void OnClickAdaptSelected(object sender, EventArgs e)
        {
            _adaptContextButton.Enabled = false;
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
                            var adaptResult = adapter.AdaptFile(child.CurrentStream, TargetPatch);
                            child.UpdateAdaptResult(adaptResult);

                            if (adaptResult.Status == MapFileStatus.Adapted)
                            {
                                _saveAsButton.Enabled = true;
                            }
                        }
                    }

                    item.Update();
                }
                else
                {
                    var adapter = tag.Adapter;
                    if (adapter != null)
                    {
                        var adaptResult = adapter.AdaptFile(tag.CurrentStream, TargetPatch);
                        tag.UpdateAdaptResult(adaptResult);

                        if (adaptResult.Status == MapFileStatus.Adapted)
                        {
                            _saveAsButton.Enabled = true;
                        }

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
            _saveAsButton.Enabled = true;
        }

        private static void TrySetOpenArchiveButtonEnabled()
        {
            _openCloseArchiveButton.Enabled = string.IsNullOrWhiteSpace(_archiveInput.Text) ? false : File.Exists(_archiveInput.Text);
        }

        private static void UpdateDiagnosticsDisplay()
        {
            if (_fileList.SelectedItems.Count == 1)
            {
                var tag = _fileList.SelectedItems[0].GetTag();
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
                _openCloseArchiveButton.Enabled = false;

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

                        var mapArchiveItem = ListViewItemExtensions.Create(new ItemTag(_archive, file, children.ToArray()));

                        _fileList.Items.Add(mapArchiveItem);
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

                _openCloseArchiveButton.Enabled = true;
                _openCloseArchiveButton.Text = "Close archive";
                _adaptAllButton.Enabled = _fileList.Items.Count > 0;
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

                if (tag.Children != null)
                {
                    if (tag.Children.Any(child => child.IsModified))
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

                            if (child.TryGetModifiedMpqFile(out var subArchiveAdaptedFile))
                            {
                                subArchiveAdaptedFiles.Add(subArchiveAdaptedFile);
                            }
                            else if (child.Status == MapFileStatus.Removed)
                            {
                                subArchiveRemovedFiles.Add(child.GetHashedFileName());
                            }
                        }

                        var subArchiveOriginalFiles = subArchive.GetMpqFiles();

                        var adaptedSubArchiveStream = new MemoryStream();
                        MpqArchive.Create(adaptedSubArchiveStream, GetCreateArchiveMpqFiles(subArchiveOriginalFiles, subArchiveAdaptedFiles, subArchiveRemovedFiles).ToArray());

                        adaptedSubArchiveStream.Position = 0;
                        var adaptedFile = MpqFile.New(adaptedSubArchiveStream, tag.FileName);
                        adaptedFile.TargetFlags = tag.MpqEntry.Flags;
                        adaptedFiles.Add(adaptedFile);
                    }
                    else if (tag.Children.All(child => child.Status == MapFileStatus.Removed))
                    {
                        removedFiles.Add(tag.GetHashedFileName());
                    }
                }
                else if (tag.TryGetModifiedMpqFile(out var adaptedFile))
                {
                    adaptedFiles.Add(adaptedFile);
                }
                else if (tag.Status == MapFileStatus.Removed)
                {
                    removedFiles.Add(tag.GetHashedFileName());
                }
            }

            MpqArchive.Create(fileName, GetCreateArchiveMpqFiles(originalFiles, adaptedFiles, removedFiles).ToArray()).Dispose();
        }

        private static IEnumerable<MpqFile> GetCreateArchiveMpqFiles(IEnumerable<MpqFile> originalFiles, IEnumerable<MpqFile> modifiedFiles, IEnumerable<ulong> removedFiles)
        {
            return modifiedFiles.Concat(originalFiles.Where(originalFile =>
                !removedFiles.Contains(originalFile.Name) &&
                !modifiedFiles.Where(modifiedFile => modifiedFile.IsSameAs(originalFile)).Any()));
        }
    }
}