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
            switch (mapDoodads.FormatVersion)
            {
                case MapWidgetsFormatVersion.Reforged:
                    mapDoodads.FormatVersion = MapWidgetsFormatVersion.Tft;
                    break;

                case MapWidgetsFormatVersion.Tft:
                    mapDoodads.FormatVersion = MapWidgetsFormatVersion.Roc;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this MapDoodads mapDoodads)
        {
            return mapDoodads.FormatVersion switch
            {
                MapWidgetsFormatVersion.Roc => GamePatch.v1_00,
                MapWidgetsFormatVersion.Tft => GamePatch.v1_07,
                MapWidgetsFormatVersion.Reforged => GamePatch.v1_32_0,
            };
        }
    }
}