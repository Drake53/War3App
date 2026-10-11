using System;

using War3Net.Build.Common;
using War3Net.Build.Widget;

namespace War3App.MapAdapter.Widget
{
    public static class MapUnitsExtensions
    {
        public static bool Adapt(this MapUnits mapUnits, AdaptFileContext context, out MapFileStatus status)
        {
            if (mapUnits.GetMinimumPatch() <= context.TargetPatch.Patch)
            {
                status = MapFileStatus.Compatible;
                return false;
            }

            status = mapUnits.TryDowngrade(context)
                ? MapFileStatus.Compatible
                : MapFileStatus.Incompatible;

            return status == MapFileStatus.Compatible;
        }

        public static bool TryDowngrade(
            this MapUnits mapUnits,
            AdaptFileContext context)
        {
            try
            {
                while (mapUnits.GetMinimumPatch() > context.TargetPatch.Patch)
                {
                    mapUnits.DowngradeOnce(context);
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

        public static void DowngradeOnce(
            this MapUnits mapUnits,
            AdaptFileContext context)
        {
            if (mapUnits.FormatVersion == MapWidgetsFormatVersion.v12 ||
                mapUnits.FormatVersion == MapWidgetsFormatVersion.v13)
            {
                mapUnits.FormatVersion = MapWidgetsFormatVersion.v8;
            }
            else if (mapUnits.UseNewFormat)
            {
                mapUnits.UseNewFormat = false;
            }
            else
            {
                mapUnits.FormatVersion = MapWidgetsFormatVersion.v7;
                mapUnits.SubVersion = MapWidgetsSubVersion.v9;
            }
        }

        public static GamePatch GetMinimumPatch(this MapUnits mapUnits)
        {
            if (mapUnits.FormatVersion == MapWidgetsFormatVersion.v12 ||
                mapUnits.FormatVersion == MapWidgetsFormatVersion.v13)
            {
                return GamePatch.v3_0_0;
            }

            if (mapUnits.UseNewFormat)
            {
                return GamePatch.v1_32_0;
            }

            return mapUnits.FormatVersion switch
            {
                MapWidgetsFormatVersion.v7 => GamePatch.v1_00,
                MapWidgetsFormatVersion.v8 => GamePatch.v1_07,
            };
        }
    }
}