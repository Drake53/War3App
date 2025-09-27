using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Extensions;
using War3App.MapAdapter.Info;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Info;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter
{
    public static class ArchiveProcessor
    {
        private static readonly GamePatch _latestPatch = Enum.GetValues<GamePatch>().Max();

        public static OpenZipArchiveResult OpenZipArchive(string filePath)
        {
            var result = new OpenZipArchiveResult();

            using var stream = File.OpenRead(filePath);
            using var zipFile = new ZipArchive(stream, ZipArchiveMode.Read);

            var encryptedAesParameters = zipFile.Entries.SingleOrDefault(e => string.Equals(e.FullName, FileName.AesParameters, StringComparison.OrdinalIgnoreCase));
            if (encryptedAesParameters is null)
            {
                var mapEntry = zipFile.Entries.Single(e => Path.GetExtension(e.FullName).StartsWith(".w3", StringComparison.OrdinalIgnoreCase));

                var mapStream = new MemoryStream();
                mapEntry.Extract(mapStream);
                mapStream.Position = 0;

                result.Archive = MpqArchive.Open(mapStream, true);
            }
            else if (OperatingSystem.IsWindows())
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

                result.Archive = MpqArchive.Open(mapStream, true);
            }
            else
            {
                throw new PlatformNotSupportedException("Decrypting MPQ archive is only supported on windows.");
            }

            var gamePatchEntry = zipFile.Entries.SingleOrDefault(e => string.Equals(e.FullName, FileName.TargetPatch, StringComparison.OrdinalIgnoreCase));
            using var gamePatchStream = gamePatchEntry.Open();

            result.TargetPatch = new TargetPatch
            {
                Patch = Enum.Parse<GamePatch>(gamePatchStream.ReadAllText()),
                GameDataPath = filePath,
                GameDataContainerType = ContainerType.ZipArchive,
            };

            return result;
        }

        public static OpenArchiveResult OpenArchive(
            MpqArchive archive,
            BackgroundWorker progressReporter)
        {
            var result = new OpenArchiveResult();

            var mapsList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (archive.IsCampaignArchive(out var campaignInfo))
            {
                for (var i = 0; i < campaignInfo.Maps.Count; i++)
                {
                    mapsList.Add(campaignInfo.Maps[i].MapFilePath);
                }
            }
            else
            {
                using var mpqStream = archive.OpenFile(MapInfo.FileName);
                using var reader = new BinaryReader(mpqStream);
                result.OriginPatch = reader.ReadMapInfo().GetOriginGamePatch();
            }

            var possibleOriginPatches = new List<GamePatch>();
            var files = archive.ToList();

            var progress = new OpenArchiveProgress();
            progress.Maximum = files.Count;

            var index = 0;

            foreach (var file in files)
            {
                if (file.FileName is not null && mapsList.Contains(file.FileName))
                {
                    var mapName = file.FileName;

                    var nestedArchiveStream = archive.OpenFile(mapName);
                    var nestedArchive = MpqArchive.Open(nestedArchiveStream, true);
                    nestedArchive.DiscoverFileNames();

                    result.NestedArchives.Add(nestedArchive);

                    var nestedFiles = nestedArchive.ToArray();
                    var nestedMapFiles = new MapFile[nestedFiles.Length];

                    progress.Maximum += nestedFiles.Length;

                    var parentIndex = index;

                    for (var i = 0; i < nestedFiles.Length; i++)
                    {
                        var nestedFile = nestedFiles[i];
                        var nestedMapFile = new MapFile(nestedArchive, nestedFile, ++index, mapName);
                        nestedMapFiles[i] = nestedMapFile;

                        progressReporter.ReportProgress(0, progress);
                    }

                    using var mapInfoFileStream = nestedArchive.OpenFile(MapInfo.FileName);
                    using var mapInfoReader = new BinaryReader(mapInfoFileStream);
                    var nestedArchiveOriginPatch = mapInfoReader.ReadMapInfo().GetOriginGamePatch();

                    if (nestedArchiveOriginPatch.HasValue)
                    {
                        possibleOriginPatches.Add(nestedArchiveOriginPatch.Value);
                    }

                    var parentMapFile = new MapFile(archive, file, parentIndex, nestedMapFiles, nestedArchiveOriginPatch);
                    result.Files.Add(parentMapFile);
                    result.Files.AddRange(nestedMapFiles);

                    progressReporter.ReportProgress(0, progress);
                }
                else
                {
                    var mapFile = new MapFile(archive, file, index);
                    result.Files.Add(mapFile);

                    progressReporter.ReportProgress(0, progress);
                }

                index++;
            }

            if (!result.OriginPatch.HasValue)
            {
                result.OriginPatch = possibleOriginPatches.Count == 1 ? possibleOriginPatches[0] : _latestPatch;
            }

            return result;
        }

        public static void SaveArchive(
            MpqArchive originalArchive,
            IEnumerable<MapFile> rootMapFiles,
            string outputFilePath,
            BackgroundWorker progressReporter)
        {
            var archiveBuilder = new MpqArchiveBuilder(originalArchive);

            var progress = new SaveArchiveProgress();
            progress.Saving = false;

            foreach (var mapFile in rootMapFiles)
            {
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
                        archiveBuilder.RemoveFile(originalArchive, mapFile.MpqEntry);
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
                        using var nestedArchive = MpqArchive.Open(originalArchive.OpenFile(mapFile.OriginalFileName));
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

                                progressReporter.ReportProgress(0, progress);
                            }
                            else
                            {
                                progressReporter.ReportProgress(0, progress);
                            }
                        }

                        var adaptedNestedArchiveStream = new MemoryStream();
                        nestedArchiveBuilder.SaveWithPreArchiveData(adaptedNestedArchiveStream, true);

                        adaptedNestedArchiveStream.Position = 0;
                        var adaptedFile = MpqFile.New(adaptedNestedArchiveStream, mapFile.CurrentFileName, false);
                        adaptedFile.TargetFlags = mapFile.MpqEntry.Flags;
                        archiveBuilder.AddFile(adaptedFile);

                        progressReporter.ReportProgress(0, progress);
                    }
                    else
                    {
                        progressReporter.ReportProgress(mapFile.Children.Length, progress);
                    }
                }
                else if (mapFile.TryGetModifiedMpqFile(out var adaptedFile))
                {
                    archiveBuilder.AddFile(adaptedFile);

                    progressReporter.ReportProgress(0, progress);
                }
                else
                {
                    progressReporter.ReportProgress(0, progress);
                }
            }

            progress.Saving = true;
            progressReporter.ReportProgress(0, progress);

            using (var fileStream = File.Create(outputFilePath))
            {
                archiveBuilder.SaveWithPreArchiveData(fileStream);
            }
        }
    }
}