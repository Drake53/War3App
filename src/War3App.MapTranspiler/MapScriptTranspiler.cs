#nullable enable

using System;
using System.ComponentModel;
using System.IO;

using CSharpLua;

using War3Net.Build;
using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Info;
using War3Net.Build.Providers;
using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Transpilers;
using War3Net.IO.Mpq;

namespace War3App.MapTranspiler
{
    public static class MapScriptTranspiler
    {
        public static void TranspileAndSave(string inputFile, string outputFile, string? commonJPath, string? blizzardJPath, ScriptLanguage targetLanguage, BackgroundWorker? worker)
        {
            var targetFileName = targetLanguage switch
            {
                ScriptLanguage.Jass => "war3map.j",
                ScriptLanguage.Lua => "war3map.lua",
                _ => throw new InvalidEnumArgumentException(nameof(targetLanguage), (int)targetLanguage, typeof(ScriptLanguage)),
            };

            using var mapArchive = MpqArchive.Open(inputFile, true);

            var map = Map.Open(mapArchive);
            var sourceLanguage = map.Info.ScriptLanguage;
            var mpqArchiveBuilder = new MpqArchiveBuilder(mapArchive);

            using var mapInfoStream = new MemoryStream();
            using var mapInfoWriter = new BinaryWriter(mapInfoStream);

            var mapInfo = map.Info;
            mapInfo.ScriptLanguage = targetLanguage;
            if (mapInfo.FormatVersion < MapInfoFormatVersion.Lua)
            {
                mapInfo.FormatVersion = MapInfoFormatVersion.Lua;
                if (mapInfo.GameVersion is null)
                {
                    mapInfo.GameVersion = GamePatchVersionProvider.GetGameVersion(GamePatch.v1_31_1);
                }
            }

            mapInfoWriter.Write(mapInfo);
            mapInfoStream.Position = 0;
            mpqArchiveBuilder.AddFile(MpqFile.New(mapInfoStream, MapInfo.FileName));

            if (sourceLanguage == ScriptLanguage.Jass)
            {
                if (targetLanguage != ScriptLanguage.Lua)
                {
                    throw new NotSupportedException($"Unable to transpile from {sourceLanguage} to {targetLanguage}.");
                }

                mpqArchiveBuilder.RemoveFile("war3map.j");
                mpqArchiveBuilder.RemoveFile(@"Scripts\war3map.j");

                var jassHelperFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Warcraft III", "JassHelper");
                if (string.IsNullOrEmpty(commonJPath))
                {
                    if (Directory.Exists(jassHelperFolder))
                    {
                        commonJPath = Path.Combine(jassHelperFolder, "common.j");
                        if (string.IsNullOrEmpty(blizzardJPath))
                        {
                            blizzardJPath = Path.Combine(jassHelperFolder, "Blizzard.j");
                        }
                    }
                    else
                    {
                        throw new DirectoryNotFoundException("Unable to automatically find common.j and Blizzard.j folder.");
                    }
                }

                var mapHasCustomBlizzardJ = mapArchive.FileExists(@"Scripts\Blizzard.j");
                if (string.IsNullOrEmpty(blizzardJPath) && !mapHasCustomBlizzardJ)
                {
                    if (Directory.Exists(jassHelperFolder))
                    {
                        blizzardJPath = Path.Combine(jassHelperFolder, "Blizzard.j");
                    }
                    else
                    {
                        throw new DirectoryNotFoundException("Unable to automatically find common.j and Blizzard.j folder.");
                    }
                }

                var estimatedWorkToTranspile = map.Script.Length + 700000;
                var estimatedWorkToRegisterCommonAndBlizzard = 35000000 / estimatedWorkToTranspile;
                if (estimatedWorkToRegisterCommonAndBlizzard == 0)
                {
                    estimatedWorkToRegisterCommonAndBlizzard = 1;
                }

                var transpiler = new JassToLuaTranspiler();
                transpiler.RegisterJassFile(JassParser.ParseFile(commonJPath));

                worker?.ReportProgress(estimatedWorkToRegisterCommonAndBlizzard);

                if (mapHasCustomBlizzardJ)
                {
                    throw new NotImplementedException("Custom Blizzard.j files are currently not supported.");
                }
                else
                {
                    transpiler.RegisterJassFile(JassParser.ParseFile(blizzardJPath));

                    worker?.ReportProgress(estimatedWorkToRegisterCommonAndBlizzard * 2);
                }

                var luaCompilationUnit = transpiler.Transpile(JassParser.ParseString(map.Script));

                using var stream = new MemoryStream();
                using (var writer = new StreamWriter(stream, leaveOpen: true))
                {
                    var luaRenderOptions = new LuaSyntaxGenerator.SettingInfo
                    {
                        // Indent = 4,
                        // IsCommentsDisabled = true,
                    };

                    var luaRenderer = new LuaRenderer(luaRenderOptions, writer);
                    luaRenderer.RenderCompilationUnit(luaCompilationUnit);
                    writer.Flush();
                }

                stream.Position = 0;
                mpqArchiveBuilder.AddFile(MpqFile.New(stream, targetFileName));

                worker?.ReportProgress(100);

                var mpqArchiveCreateOptions = new MpqArchiveCreateOptions
                {
                };

                mpqArchiveBuilder.SaveTo(outputFile, mpqArchiveCreateOptions);
            }
            else if (sourceLanguage == ScriptLanguage.Lua)
            {
                mpqArchiveBuilder.RemoveFile("war3map.lua");
                mpqArchiveBuilder.RemoveFile(@"Scripts\war3map.lua");

                throw new NotSupportedException($"Unable to transpile from {sourceLanguage} to {targetLanguage}.");
            }
            else
            {
                throw new NotSupportedException($"Unable to transpile from {sourceLanguage} to {targetLanguage}.");
            }
        }
    }
}