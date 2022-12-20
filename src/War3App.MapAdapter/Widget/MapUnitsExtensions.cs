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
                mapUnits.FormatVersion = MapWidgetsFormatVersion.v7;
                mapUnits.SubVersion = MapWidgetsSubVersion.v9;
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
                MapWidgetsFormatVersion.v7 => GamePatch.v1_00,
                MapWidgetsFormatVersion.v8 => GamePatch.v1_07,
            };
        }
    }
}