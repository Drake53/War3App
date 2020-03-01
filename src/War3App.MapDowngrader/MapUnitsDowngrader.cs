using System;

using War3Net.Build.Common;
using War3Net.Build.Widget;

namespace War3App.MapDowngrader
{
    public static class MapUnitsDowngrader
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
            switch (mapUnits.FormatVersion)
            {
                case MapWidgetsFormatVersion.Reforged:
                    mapUnits.FormatVersion = MapWidgetsFormatVersion.Tft;
                    break;

                case MapWidgetsFormatVersion.Tft:
                    mapUnits.FormatVersion = MapWidgetsFormatVersion.Roc;
                    break;

                default: break;
            }
        }

        public static GamePatch GetMinimumPatch(this MapUnits mapUnits)
        {
            return mapUnits.FormatVersion switch
            {
                MapWidgetsFormatVersion.Roc => GamePatch.v1_00,
                MapWidgetsFormatVersion.Tft => GamePatch.v1_07,
                MapWidgetsFormatVersion.Reforged => GamePatch.v1_32_0,
            };
        }
    }
}