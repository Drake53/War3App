using System;

using War3Net.Build.Common;
using War3Net.Build.Info;
using War3Net.Build.Providers;

namespace War3App.MapAdapter.Info
{
    public static class MapInfoExtensions
    {
        public static bool Adapt(this MapInfo mapInfo, AdaptFileContext context, out MapFileStatus status)
        {
            if (mapInfo.GetMinimumPatch() <= context.TargetPatch.Patch)
            {
                status = MapFileStatus.Compatible;
                return false;
            }

            status = mapInfo.TryDowngrade(context.TargetPatch.Patch)
                ? MapFileStatus.Compatible
                : MapFileStatus.Incompatible;

            return status == MapFileStatus.Compatible;
        }

        public static GamePatch? GetOriginGamePatch(this MapInfo mapInfo)
        {
            var gameBuilds = mapInfo.GameVersion is not null
                ? GameBuildsProvider.GetGameBuilds(mapInfo.GameVersion)
                : GameBuildsProvider.GetGameBuilds(mapInfo.EditorVersion);

            return gameBuilds.Count == 1 ? gameBuilds[0].GamePatch : null;
        }

        public static bool TryDowngrade(this MapInfo mapInfo, GamePatch targetPatch)
        {
            try
            {
                while (mapInfo.GetMinimumPatch() > targetPatch)
                {
                    mapInfo.DowngradeOnce();
                }

                var targetGameBuild = GameBuildsProvider.GetGameBuilds(targetPatch)[0];

                mapInfo.EditorVersion = targetGameBuild.EditorVersion.Value;
                if (mapInfo.FormatVersion >= MapInfoFormatVersion.v28)
                {
                    mapInfo.GameVersion = targetGameBuild.Version;
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this MapInfo mapInfo)
        {
            switch (mapInfo.FormatVersion)
            {
                case MapInfoFormatVersion.v33:
                    mapInfo.MapFlags &= ~(MapFlags.DisableDenyIcon | MapFlags.ForceDefaultCameraZoom | MapFlags.ForceMaxCameraZoom | MapFlags.ForceMinCameraZoom);
                    mapInfo.FormatVersion = MapInfoFormatVersion.v31;
                    break;

                case MapInfoFormatVersion.v31:
                    mapInfo.MapFlags &= ~(MapFlags.AccurateProbabilityForCalculations | MapFlags.CustomAbilitySkin);
                    mapInfo.FormatVersion = MapInfoFormatVersion.v28;
                    break;

                case MapInfoFormatVersion.v28:
                    if (mapInfo.ScriptLanguage == ScriptLanguage.Lua)
                    {
                        throw new NotSupportedException("Cannot downgrade, because map is set to use Lua as script language.");
                    }

                    mapInfo.GameVersion = null;
                    mapInfo.FormatVersion = MapInfoFormatVersion.v25;
                    break;

                case MapInfoFormatVersion.v25:
                    if (mapInfo.Players.Count > 12 || mapInfo.Forces.Count > 12)
                    {
                        throw new NotSupportedException("Cannot downgrade, because the map uses more than 12 players and/or teams.");
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this MapInfo mapInfo)
        {
            var minimumBySlotCounts = mapInfo.Players.Count > 12 || mapInfo.Forces.Count > 12 ? GamePatch.v1_29_0 : GamePatch.v1_00;
            var minimumByFormatVersion = mapInfo.FormatVersion switch
            {
                MapInfoFormatVersion.v18 => GamePatch.v1_00,
                MapInfoFormatVersion.v25 => GamePatch.v1_07,
                MapInfoFormatVersion.v28 => GamePatch.v1_31_0,
                MapInfoFormatVersion.v31 => GamePatch.v1_32_0,
                MapInfoFormatVersion.v33 => GamePatch.v2_0_3,

                MapInfoFormatVersion.v8 => GamePatch.v1_00,
                MapInfoFormatVersion.v10 => GamePatch.v1_00,
                MapInfoFormatVersion.v11 => GamePatch.v1_00,
                MapInfoFormatVersion.v15 => GamePatch.v1_00,
                MapInfoFormatVersion.v23 => GamePatch.v1_07,
                MapInfoFormatVersion.v24 => GamePatch.v1_07,
                MapInfoFormatVersion.v26 => GamePatch.v1_31_0,
                MapInfoFormatVersion.v27 => GamePatch.v1_31_0,
                MapInfoFormatVersion.v32 => GamePatch.v2_0_3,
            };

            return minimumBySlotCounts > minimumByFormatVersion ? minimumBySlotCounts : minimumByFormatVersion;
        }
    }
}