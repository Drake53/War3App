//#define SKIP_SCRIPT_EDITING
#define DOWNGRADE_WE_ONLY_FILES

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using War3Net.Build.Audio;
using War3Net.Build.Common;
using War3Net.Build.Info;
using War3Net.Build.Widget;
using War3Net.IO.Mpq;

namespace War3App.MapDowngrader
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
#if DEBUG
            var inputMapPath = @"";
            var outputFolder = @"";
            var targetPatch = GamePatch.v1_31_0;
#else
            throw new NotImplementedException();

            // TODO: get input/output from args
            var inputMapPath = "";
            var outputFolder = "";
            var targetPatch = GamePatch.;
#endif

            var mapName = new FileInfo(inputMapPath).Name;
            var newMapName = mapName[0..^4] + " for 1.31.w3x";
            var outputMapFilePath = Path.Combine(outputFolder, newMapName);

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            using (var inputArchive = MpqArchive.Open(inputMapPath, true))
            {
                inputArchive.AddFilename("(attributes)");
                inputArchive.AddFilename("conversation.json");

                var inputFiles = inputArchive.GetMpqFiles();
                var replacedFiles = new List<MpqFile>();

                // Downgrade MapInfo
                var mapInfo = MapInfo.Parse(inputArchive.OpenFile(MapInfo.FileName), false);
                if (!mapInfo.TryDowngrade(targetPatch))
                {
                    return;
                }

                var mapInfoFile = GetSerializedMapFile(mapInfo, MapInfo.Serialize, MapInfo.FileName);

                // Downgrade MapDoodads
                MpqFile mapDoodadsFile = null;
#if DOWNGRADE_WE_ONLY_FILES
                if (inputArchive.FileExists(MapDoodads.FileName))
                {
                    var mapDoodads = MapDoodads.Parse(inputArchive.OpenFile(MapDoodads.FileName), false);
                    if (!mapDoodads.TryDowngrade(targetPatch))
                    {
                        return;
                    }

                    mapDoodadsFile = GetSerializedMapFile(mapDoodads, MapDoodads.Serialize, MapDoodads.FileName);
                }
#endif

                // Downgrade MapUnits
                MpqFile mapUnitsFile = null;
#if DOWNGRADE_WE_ONLY_FILES
                if (inputArchive.FileExists(MapUnits.FileName))
                {
                    var mapUnits = MapUnits.Parse(inputArchive.OpenFile(MapUnits.FileName), false);
                    if (!mapUnits.TryDowngrade(targetPatch))
                    {
                        return;
                    }

                    mapUnitsFile = GetSerializedMapFile(mapUnits, MapUnits.Serialize, MapUnits.FileName);
                }
#endif

                // Downgrade MapSounds
                MpqFile mapSoundsFile = null;
#if DOWNGRADE_WE_ONLY_FILES
                if (inputArchive.FileExists(MapSounds.FileName))
                {
                    var mapSounds = MapSounds.Parse(inputArchive.OpenFile(MapSounds.FileName), false);
                    if (!mapSounds.TryDowngrade(targetPatch))
                    {
                        return;
                    }

                    mapSoundsFile = GetSerializedMapFile(mapSounds, MapSounds.Serialize, MapSounds.FileName);
                }
#endif

                // Verify MapScript
                FileStream scriptFileStream = null;
                if (mapInfo.ScriptLanguage == ScriptLanguage.Jass)
                {
                    const string OriginalScriptFileName = "war3map.original.j";
                    const string ScriptFileName = "war3map.j";

                    var originalScriptFilePath = Path.Combine(outputFolder, OriginalScriptFileName);
                    var scriptFilePath = Path.Combine(outputFolder, ScriptFileName);

                    var incompatibleIdentifiersUsage = new Dictionary<string, int>();
                    var incompatibleIdentifiers = new HashSet<string>();

                    //var originPatch = mapInfo.GameVersion
                    var originPatch = GamePatch.v1_32_1; // always use latest for now...
                    var mapHasCustomBlizzardJ = false; // assume no custom Blizzard.j for now (if a map does have one, treat it like another war3map.j by downgrading it)

                    incompatibleIdentifiers.UnionWith(CommonIdentifiersProvider.GetIdentifiers(targetPatch, originPatch));
                    if (!mapHasCustomBlizzardJ)
                    {
                        incompatibleIdentifiers.UnionWith(BlizzardIdentifiersProvider.GetIdentifiers(targetPatch, originPatch));
                    }

                    using (var scriptFile = inputArchive.OpenFile(ScriptFileName))
                    {
                        using (var reader = new StreamReader(scriptFile, leaveOpen: true))
                        {
                            var scriptText = reader.ReadToEnd();
                            foreach (var incompatibleIdentifier in incompatibleIdentifiers)
                            {
                                var usageCount = new Regex(incompatibleIdentifier).Matches(scriptText).Count;
                                if (usageCount > 0)
                                {
                                    incompatibleIdentifiersUsage.Add(incompatibleIdentifier, usageCount);
                                }
                            }
                        }

                        if (!File.Exists(originalScriptFilePath))
                        {
                            scriptFile.Position = 0;
                            using (var fileStream = new FileStream(originalScriptFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                scriptFile.CopyTo(fileStream);
                            }
                        }

                        if (!File.Exists(scriptFilePath))
                        {
                            scriptFile.Position = 0;
                            using (var fileStream = new FileStream(scriptFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                scriptFile.CopyTo(fileStream);
                            }
                        }
                    }

#if !SKIP_SCRIPT_EDITING
                    if (incompatibleIdentifiersUsage.Count > 0)
                    {
                        Console.WriteLine("Before the program can continue, you must manually edit the map script to make it compatible.");
                        Console.WriteLine("The following identifiers were detected which are not compatible with the target version:");
                        Console.WriteLine();

                        // NOTE: results can be duplicate/incorrect (for example: when you have CreateCommandButtonEffectBJ, it will also detect CreateCommandButtonEffect)
                        foreach (var incompatibleIdentifier in incompatibleIdentifiersUsage)
                        {
                            Console.WriteLine($"{incompatibleIdentifier.Key} (used {incompatibleIdentifier.Value} times)");
                        }

                        Process.Start("explorer.exe", $"/select, \"{scriptFilePath}\"");

                        Console.WriteLine();
                        Console.WriteLine("After you're done, press any key to continue.");
                        Console.ReadKey();
                    }
#endif

                    scriptFileStream = File.OpenRead(scriptFilePath);
                    replacedFiles.Add(MpqFile.New(scriptFileStream, ScriptFileName));
                }
                else
                {
                    throw new NotImplementedException();
                }

                // Verify Assets
                foreach (var mpqEntry in inputArchive)
                {
                    using var mpqStream = inputArchive.OpenFile(mpqEntry);
                    if (mpqStream.Length < 4)
                    {
                        continue;
                    }

                    using var reader = new BinaryReader(mpqStream);
                    char[] chars;
                    try
                    {
                        chars = reader.ReadChars(4);
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }

                    var mdxMagic = new string(chars);
                    if (mdxMagic != "MDLX")
                    {
                        continue;
                    }

                    while (mpqStream.Length - mpqStream.Position > 8)
                    {
                        // Find VERS chunk.
                        var chunkTag = new string(reader.ReadChars(4));
                        var chunkSize = reader.ReadInt32();
                        if (chunkTag == "VERS")
                        {
                            var version = reader.ReadUInt32();
                            if (version > 800 && targetPatch < GamePatch.v1_32_0)
                            {
                                throw new Exception("Map contains reforged models.");
                            }

                            continue;
                        }
                        else
                        {
                            reader.ReadBytes(chunkSize);
                        }
                    }
                }

                // Rebuild
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }
                else if (File.Exists(outputMapFilePath))
                {
                    File.Delete(outputMapFilePath);
                }

                replacedFiles.AddRange(new[]
                {
                    mapInfoFile,
                    mapDoodadsFile,
                    mapUnitsFile,
                    mapSoundsFile,
                }.Where(file => file != null));

                var nonReplacedFiles = inputFiles.Where(mpqFile => !replacedFiles.Where(replacedFile => replacedFile.IsSameAs(mpqFile)).Any());
                var newFiles = nonReplacedFiles.Concat(replacedFiles).ToArray();

                MpqArchive.Create(outputMapFilePath, newFiles).Dispose();
                scriptFileStream?.Dispose();
            }
        }

        private static MpqFile GetSerializedMapFile<TMapFile>(TMapFile mapFile, Action<TMapFile, Stream, bool> serializer, string fileName)
        {
            var memoryStream = new MemoryStream();
            serializer(mapFile, memoryStream, true);
            memoryStream.Position = 0;
            return MpqFile.New(memoryStream, fileName);
        }
    }
}