using System;

using War3Net.Build.Common;
using War3Net.Build.Widget;

namespace War3App.MapAdapter.Widget
{
    public static class MapUnitsExtensions
    {
        public static bool TryDowngrade(this MapUnits mapUnits, GamePatch targetPatch)
        {
            try
            {
                while (mapUnits.GetMinimumPatch() > targetPatch)
                {
                    mapUnits.DowngradeOnce();
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

        public static void DowngradeOnce(this MapUnits mapUnits)
        {
            if (mapUnits.UseNewFormat)
            {
                mapUnits.UseNewFormat = false;
            }
            else
            {
                mapUnits.FormatVersion = MapWidgetsFormatVersion.RoC;
                mapUnits.SubVersion = MapWidgetsSubVersion.V9;
            }
        }

        public static GamePatch GetMinimumPatch(this MapUnits mapUnits)
        {
            if (mapUnits.UseNewFormat)
            {
                return GamePatch.v1_32_0;
            }

            return mapUnits.FormatVersion switch
            {
                MapWidgetsFormatVersion.RoC => GamePatch.v1_00,
                MapWidgetsFormatVersion.TFT => GamePatch.v1_07,
            };
        }
    }
}