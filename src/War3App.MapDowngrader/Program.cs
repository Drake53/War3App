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
        internal static int FromRawcode(this string code)
        {
            if ((code?.Length ?? 0) != 4)
            {
                return 0;
            }

            return (code[0]) | (code[1] << 8) | (code[2] << 16) | (code[3] << 24);
        }

        private static readonly Dictionary<string, GamePatch> _gamePatchMappings = new Dictionary<string, GamePatch>()
        {
            { "1.27", GamePatch.v1_27a },
            { "1.28", GamePatch.v1_28 },
            { "1.29", GamePatch.v1_29_0 },
            { "1.30", GamePatch.v1_30_0 },

            { "1.31", GamePatch.v1_31_0 },
            { "1.31.0", GamePatch.v1_31_0 },
            { "1.31.1", GamePatch.v1_31_1 },

            { "1.32", GamePatch.v1_32_0 },
            { "1.32.0", GamePatch.v1_32_0 },
            { "1.32.1", GamePatch.v1_32_1 },
        };

        private static void Main(string[] args)
        {
#if DEBUG
            var inputMapPath = @"";
            var outputFolder = @"";

            var targetPatch = GamePatch.v1_31_0;

            var originPatchString = "1.32.1";
            var originPatch = GamePatch.v1_32_1;
#else
            var targetPatchString = string.Empty;
            var originPatchString = string.Empty;
            if (args.Length != 3 && args.Length != 4)
            {
                Console.WriteLine("Expect three or four arguments:");
                Console.WriteLine("input map file");
                Console.WriteLine("output folder");
                Console.WriteLine("target patch");
                Console.WriteLine("[optional] origin patch");
                Console.WriteLine();
                Console.WriteLine("Examples:");
                Console.WriteLine("downgrader.exe \"C:\\input\\file.w3x\" \"C:\\output\\folder\" \"1.31\"");
                Console.WriteLine("downgrader.exe \"C:\\input\\file.w3x\" \"C:\\output\\folder\" \"1.31\" \"1.32\"");
            }
            else
            {
                targetPatchString = args[2];
                if (args.Length == 4)
                {
                    originPatchString = args[3];
                }
            }

            var validTargetPatch = _gamePatchMappings.TryGetValue(targetPatchString, out var targetPatch);
            var validOriginPatch = _gamePatchMappings.TryGetValue(originPatchString, out var originPatch);

            if (args.Length == 3)
            {
                validOriginPatch = true;
                originPatch = GamePatch.v1_32_1;
            }

            if (!validTargetPatch || !validOriginPatch)
            {
                if (!validTargetPatch && (args.Length == 3 || args.Length == 4))
                {
                    Console.WriteLine("Invalid target patch.");
                }

                if (!validOriginPatch && args.Length == 4)
                {
                    Console.WriteLine("Invalid origin patch.");
                }

                Console.WriteLine("Supported target patches:");
                foreach (var patch in _gamePatchMappings)
                {
                    Console.WriteLine(patch.Key);
                }

                return;
            }

            var inputMapPath = args[0];
            var outputFolder = args[1];
#endif

            var mapName = new FileInfo(inputMapPath).Name;
            var newMapName = mapName[0..^4] + $" for {targetPatch}.w3x";
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

                // Detect origin patch
                if (originPatchString == string.Empty)
                {
                    if (mapInfo.GameVersion != null)
                    {
                        if (mapInfo.GameVersion.Major == 1)
                        {
                            if (mapInfo.GameVersion.Minor == 31)
                            {
                                if (mapInfo.GameVersion.Build == 0)
                                {
                                    originPatch = GamePatch.v1_31_0;
                                }
                                else if (mapInfo.GameVersion.Build == 1)
                                {
                                    originPatch = GamePatch.v1_31_1;
                                }
                            }
                            else if (mapInfo.GameVersion.Minor == 32)
                            {
                                if (mapInfo.GameVersion.Build == 0)
                                {
                                    originPatch = GamePatch.v1_32_0;
                                }
                                else if (mapInfo.GameVersion.Build == 1)
                                {
                                    originPatch = GamePatch.v1_32_1;
                                }
                                else if (mapInfo.GameVersion.Build < 4)
                                {
                                    // todo
                                    originPatch = GamePatch.v1_32_1;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (mapInfo.EditorVersion >= 6090 && mapInfo.EditorVersion < 7000)
                        {
                            originPatch = GamePatch.v1_32_0;
                        }
                        else if (mapInfo.EditorVersion == 6072)
                        {
                            originPatch = GamePatch.v1_31_0;
                        }
                        else if (mapInfo.EditorVersion == 6061)
                        {
                            originPatch = GamePatch.v1_30_0;
                        }
                        else if (mapInfo.EditorVersion == 6060)
                        {
                            originPatch = GamePatch.v1_29_0;
                        }
                        else if (mapInfo.EditorVersion == 6059)
                        {
                            originPatch = GamePatch.v1_28_5;
                        }
                    }
                }

                if (targetPatch == originPatch)
                {
                    Console.WriteLine($"Target and origin patch are the same: {targetPatch}");
                    return;
                }

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
                var useLua = mapInfo.ScriptLanguage == ScriptLanguage.Lua;
                var scriptFileExtension = useLua ? "lua" : "j";
                var originalScriptFileName = $"war3map.original.{scriptFileExtension}";
                var scriptFileName = $"war3map.{scriptFileExtension}";

                // TODO: parse jass/lua syntax instead of using regex (though as long as using regex, the code for jass and lua below can be the same)
                {
                    var originalScriptFilePath = Path.Combine(outputFolder, originalScriptFileName);
                    var scriptFilePath = Path.Combine(outputFolder, scriptFileName);

                    var incompatibleIdentifiersUsage = new Dictionary<string, int>();
                    var incompatibleIdentifiers = new HashSet<string>();

                    // assume no custom Blizzard.j for now (if a map does have one, treat it like another war3map.j by downgrading it)
                    // TODO: test if custom 'Blizzard.j' is possible for lua maps
                    var mapHasCustomBlizzardJ = false;

                    incompatibleIdentifiers.UnionWith(CommonIdentifiersProvider.GetIdentifiers(targetPatch, originPatch));
                    if (!mapHasCustomBlizzardJ)
                    {
                        incompatibleIdentifiers.UnionWith(BlizzardIdentifiersProvider.GetIdentifiers(targetPatch, originPatch));
                    }

                    using (var scriptFile = inputArchive.OpenFile(scriptFileName))
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
                        Console.WriteLine($"The following identifiers were detected which are not compatible with the target version '{targetPatch}':");
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
                    replacedFiles.Add(MpqFile.New(scriptFileStream, scriptFileName));

                    if (!useLua)
                    {
                        // TODO: validate map script using jasshelper?
                        // - passed common.j and Blizzard.j depend on target patch
                        // - if custom blizzard.j is used, pass that one instead

                        // var jasshelper = Process.Start(...);
                        // jasshelper.WaitForExit();

                        // if (jasshelper.ExitCode != 0) return;
                    }
                }

                // Verify object data
                bool ValidateObjectData(string fileName, Func<Stream, GamePatch, bool> validatorFunc)
                {
                    if (inputArchive.FileExists(fileName))
                    {
                        if (targetPatch == GamePatch.v1_29_0 ||
                            targetPatch == GamePatch.v1_29_1 ||
                            targetPatch == GamePatch.v1_29_2 ||
                            targetPatch == GamePatch.v1_31_0 ||
                            targetPatch == GamePatch.v1_31_1)
                        {
                            Console.WriteLine($"Checking '{fileName}'...");
                            using var fileStream = inputArchive.OpenFile(fileName);
                            return validatorFunc(fileStream, targetPatch);
                        }
                        else
                        {
                            Console.WriteLine($"Skipping '{fileName}' check, because this functionality is not supported for target patch '{targetPatch}.");
                        }
                    }

                    return true;
                }

                if (!ValidateObjectData("war3map.w3u", UnitObjectDataValidator.TryValidate) |
                    !ValidateObjectData("war3map.w3t", ItemObjectDataValidator.TryValidate) |
                    !ValidateObjectData("war3map.w3b", DestructableObjectDataValidator.TryValidate) |
                    !ValidateObjectData("war3map.w3d", DoodadObjectDataValidator.TryValidate) |
                    !ValidateObjectData("war3map.w3a", AbilityObjectDataValidator.TryValidate) |
                    !ValidateObjectData("war3map.w3h", BuffObjectDataValidator.TryValidate) |
                    !ValidateObjectData("war3map.w3q", UpgradeObjectDataValidator.TryValidate))
                {
                    return;
                }

                // Verify Assets
                // TODO: textures? (.tga/.blp/.dds)
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

                using var newArchive = MpqArchive.Create(outputMapFilePath, newFiles);

#if DEBUG
                foreach (var mpqEntry in newArchive)
                {
                    if (mpqEntry.Filename == null)
                    {
                        continue;
                    }

                    using var mpqStream = newArchive.OpenFile(mpqEntry);
                    var outputFilePath = Path.Combine(outputFolder, "files", mpqEntry.Filename);
                    var outputFileDirectoryInfo = new FileInfo(outputFilePath).Directory;
                    if (!outputFileDirectoryInfo.Exists)
                    {
                        outputFileDirectoryInfo.Create();
                    }

                    using var fileStream = File.Create(outputFilePath);
                    mpqStream.CopyTo(fileStream);
                }
#endif

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