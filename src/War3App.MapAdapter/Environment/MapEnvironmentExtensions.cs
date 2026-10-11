using System;
using System.Linq;

using War3Net.Build.Common;
using War3Net.Build.Environment;

namespace War3App.MapAdapter.Environment
{
    public static class MapEnvironmentExtensions
    {
        public static bool Adapt(this MapEnvironment mapEnvironment, AdaptFileContext context, out MapFileStatus status)
        {
            if (mapEnvironment.GetMinimumPatch() <= context.TargetPatch.Patch)
            {
                status = MapFileStatus.Compatible;
                return false;
            }

            status = mapEnvironment.TryDowngrade(context)
                ? MapFileStatus.Compatible
                : MapFileStatus.Incompatible;

            return status == MapFileStatus.Compatible;
        }

        public static bool TryDowngrade(
            this MapEnvironment mapEnvironment,
            AdaptFileContext context)
        {
            try
            {
                while (mapEnvironment.GetMinimumPatch() > context.TargetPatch.Patch)
                {
                    mapEnvironment.DowngradeOnce(context);
                }

                if (EnumSupport.GetMinimumPatch(mapEnvironment.Tileset) > context.TargetPatch.Patch)
                {
                    throw new NotSupportedException("Cannot downgrade, because the map uses an unsupported tileset.");
                }

                if (mapEnvironment.TerrainTypes.Any(terrainType => EnumSupport.GetMinimumPatch(terrainType) > context.TargetPatch.Patch))
                {
                    throw new NotSupportedException("Cannot downgrade, because the map uses one or more unsupported terrain types.");
                }

                if (mapEnvironment.CliffTypes.Any(cliffType => EnumSupport.GetMinimumPatch(cliffType) > context.TargetPatch.Patch))
                {
                    throw new NotSupportedException("Cannot downgrade, because the map uses one or more unsupported cliff types.");
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(
            this MapEnvironment mapEnvironment,
            AdaptFileContext context)
        {
            switch (mapEnvironment.FormatVersion)
            {
                case MapEnvironmentFormatVersion.v12:
                    mapEnvironment.FormatVersion = MapEnvironmentFormatVersion.v11;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this MapEnvironment mapEnvironment)
        {
            return mapEnvironment.FormatVersion switch
            {
                MapEnvironmentFormatVersion.v11 => GamePatch.v1_00,
                MapEnvironmentFormatVersion.v12 => GamePatch.v2_0_3,
            };
        }
    }
}