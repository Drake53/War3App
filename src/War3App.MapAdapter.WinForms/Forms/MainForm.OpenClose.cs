#define USE_KEY_CONTAINER

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Extensions;
using War3App.MapAdapter.Info;
using War3App.MapAdapter.WinForms.Extensions;
using War3App.MapAdapter.WinForms.Helpers;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Info;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.WinForms.Forms
{
    partial class MainForm
    {
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

        private BackgroundWorker CreateOpenArchiveWorker()
        {
            var worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false,
            };

            worker.DoWork += OpenArchiveBackgroundWork;
            worker.ProgressChanged += OpenArchiveProgressChanged;
            worker.RunWorkerCompleted += OpenArchiveCompleted;

            return worker;
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
    }
}