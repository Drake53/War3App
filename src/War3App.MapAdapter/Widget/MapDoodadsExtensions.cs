using System;

using War3Net.Build.Common;
using War3Net.Build.Widget;

namespace War3App.MapAdapter.Widget
{
    public static class MapDoodadsExtensions
    {
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
                mapDoodads.FormatVersion = MapWidgetsFormatVersion.RoC;
                mapDoodads.SubVersion = MapWidgetsSubVersion.V9;
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
                MapWidgetsFormatVersion.RoC => GamePatch.v1_00,
                MapWidgetsFormatVersion.TFT => GamePatch.v1_07,
            };
        }
    }
}