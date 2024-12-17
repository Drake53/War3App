using System;

using War3Net.Build.Common;
using War3Net.Build.Widget;

namespace War3App.MapAdapter.Widget
{
    public static class MapDoodadsExtensions
    {
        public static MapFileStatus Adapt(this MapDoodads mapDoodads, AdaptFileContext context)
        {
            if (mapDoodads.GetMinimumPatch() <= context.TargetPatch.Patch)
            {
                return MapFileStatus.Compatible;
            }

            return mapDoodads.TryDowngrade(context.TargetPatch.Patch)
                ? MapFileStatus.Adapted
                : MapFileStatus.Incompatible;
        }

        public static bool TryDowngrade(this MapDoodads mapDoodads, GamePatch targetPatch)
        {
            try
            {
                while (mapDoodads.GetMinimumPatch() > targetPatch)
                {
                    mapDoodads.DowngradeOnce();
                }

                return true;
            }
            catch (NotSupportedException)
            {
                return false;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this MapDoodads mapDoodads)
        {
            if (mapDoodads.UseNewFormat)
            {
                mapDoodads.UseNewFormat = false;
            }
            else
            {
                mapDoodads.FormatVersion = MapWidgetsFormatVersion.v7;
                mapDoodads.SubVersion = MapWidgetsSubVersion.v9;
            }
        }

        public static GamePatch GetMinimumPatch(this MapDoodads mapDoodads)
        {
            if (mapDoodads.UseNewFormat)
            {
                return GamePatch.v1_32_0;
            }

            return mapDoodads.FormatVersion switch
            {
                MapWidgetsFormatVersion.v7 => GamePatch.v1_00,
                MapWidgetsFormatVersion.v8 => GamePatch.v1_07,
            };
        }
    }
}