using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

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

        public static OpenArchiveResult OpenArchive(
            MpqArchive archive,
            BackgroundWorker progressReporter)
        {
            var result = new OpenArchiveResult();

            var mapsList = new HashSet<string>();

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
    }
}