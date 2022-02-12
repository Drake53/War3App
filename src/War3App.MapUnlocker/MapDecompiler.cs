using System;
using System.ComponentModel;
using System.Linq;

using War3Net.Build;
using War3Net.Build.Audio;
using War3Net.Build.Common;
using War3Net.Build.Environment;
using War3Net.Build.Providers;
using War3Net.Build.Script;
using War3Net.CodeAnalysis.Decompilers;
using War3Net.Common.Extensions;

namespace War3App.MapUnlocker
{
    public static class MapDecompiler
    {
        public static MapFiles AutoDetectMapFilesToDecompile(Map map)
        {
            var filesToDecompile = (MapFiles)0;

            if (map.Sounds is null)
            {
                filesToDecompile |= MapFiles.Sounds;
            }

            if (map.Cameras is null)
            {
                filesToDecompile |= MapFiles.Cameras;
            }

            if (map.Regions is null || map.Regions.Protected)
            {
                filesToDecompile |= MapFiles.Regions;
            }

            if (map.Triggers is null || !map.Triggers.TriggerItems.Any(triggerItem => triggerItem is TriggerDefinition))
            {
                filesToDecompile |= MapFiles.Triggers;
            }

            return filesToDecompile;
        }

        /// <returns>A <b>new</b> <see cref="Map"/> object containing <b>only</b> the decompiled map files.</returns>
        public static Map DecompileMap(Map map, MapFiles filesToDecompile, BackgroundWorker? worker)
        {
            if (!filesToDecompile.IsDefined(allowNoFlags: false))
            {
                throw new InvalidEnumArgumentException(nameof(filesToDecompile), (int)filesToDecompile, typeof(MapFiles));
            }

            if (filesToDecompile.HasFlag(MapFiles.Environment)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.PathingMap)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.PreviewIcons)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.ShadowMap)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.ImportedFiles)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.Info)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.AbilityObjectData)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.BuffObjectData)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.DestructableObjectData)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.DoodadObjectData)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.ItemObjectData)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.UnitObjectData)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.UpgradeObjectData)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.CustomTextTriggers)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.Script)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.TriggerStrings)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.Doodads)) throw new NotImplementedException();
            if (filesToDecompile.HasFlag(MapFiles.Units)) throw new NotImplementedException();

            var decompiler = new JassScriptDecompiler(map);
            var decompiledMap = new Map();

            var decompiledCount = 0;
            var toDecompileCount = 0;
            var progress = 0;

            if (filesToDecompile.HasFlag(MapFiles.Sounds)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.Cameras)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.Environment)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.PathingMap)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.PreviewIcons)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.Regions)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.ShadowMap)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.ImportedFiles)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.Info)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.AbilityObjectData)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.BuffObjectData)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.DestructableObjectData)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.DoodadObjectData)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.ItemObjectData)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.UnitObjectData)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.UpgradeObjectData)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.CustomTextTriggers)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.Script)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.Triggers)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.TriggerStrings)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.Doodads)) toDecompileCount++;
            if (filesToDecompile.HasFlag(MapFiles.Units)) toDecompileCount++;

            var gameVersion = map.Info?.GameVersion;
            var gamePatch = gameVersion is null ? GamePatch.v1_26a : GamePatchVersionProvider.GetGamePatch(gameVersion);

            if (filesToDecompile.HasFlag(MapFiles.Sounds))
            {
                worker?.ReportProgress(progress, "Decompiling sounds...");

                var formatVersion = gamePatch >= GamePatch.v1_32_6
                    ? MapSoundsFormatVersion.ReforgedV3
                    : gamePatch >= GamePatch.v1_32_0
                        ? MapSoundsFormatVersion.Reforged
                        : MapSoundsFormatVersion.Normal;

                if (decompiler.TryDecompileMapSounds(formatVersion, out var decompiledMapSounds))
                {
                    decompiledMap.Sounds = decompiledMapSounds;
                }

                decompiledCount++;
                progress = (100 * decompiledCount) / toDecompileCount;
            }

            if (filesToDecompile.HasFlag(MapFiles.Cameras))
            {
                worker?.ReportProgress(progress, "Decompiling cameras...");

                var formatVersion = MapCamerasFormatVersion.Normal;
                var useNewFormat = gamePatch >= GamePatch.v1_31_0;

                if (decompiler.TryDecompileMapCameras(formatVersion, useNewFormat, out var decompiledMapCameras))
                {
                    decompiledMap.Cameras = decompiledMapCameras;
                }

                decompiledCount++;
                progress = (100 * decompiledCount) / toDecompileCount;
            }

            if (filesToDecompile.HasFlag(MapFiles.Regions))
            {
                worker?.ReportProgress(progress, "Decompiling regions...");

                var formatVersion = MapRegionsFormatVersion.Normal;

                if (decompiler.TryDecompileMapRegions(formatVersion, out var decompiledMapRegions))
                {
                    decompiledMap.Regions = decompiledMapRegions;
                }

                decompiledCount++;
                progress = (100 * decompiledCount) / toDecompileCount;
            }

            if (filesToDecompile.HasFlag(MapFiles.Triggers))
            {
                worker?.ReportProgress(progress, "Decompiling triggers...");

                var formatVersion = gamePatch >= GamePatch.v1_07 ? MapTriggersFormatVersion.Tft : MapTriggersFormatVersion.RoC;
                var subVersion = gamePatch >= GamePatch.v1_31_0 ? (MapTriggersSubVersion?)MapTriggersSubVersion.New : null;

                if (decompiler.TryDecompileMapTriggers(formatVersion, subVersion, out var decompiledMapTriggers))
                {
                    decompiledMap.Triggers = decompiledMapTriggers;
                }

                decompiledCount++;
                progress = (100 * decompiledCount) / toDecompileCount;
            }

            worker?.ReportProgress(progress, "Done");

            return decompiledMap;
        }
    }
}