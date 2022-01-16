using System;

using War3Net.Build.Common;
using War3Net.Build.Script;

namespace War3App.MapAdapter.Script
{
    public static class MapCustomTextTriggersExtensions
    {
        public static bool TryDowngrade(this MapCustomTextTriggers mapCustomTextTriggers, GamePatch targetPatch)
        {
            try
            {
                while (mapCustomTextTriggers.GetMinimumPatch() > targetPatch)
                {
                    mapCustomTextTriggers.DowngradeOnce();
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

        public static void DowngradeOnce(this MapCustomTextTriggers mapCustomTextTriggers)
        {
            if (mapCustomTextTriggers.SubVersion.HasValue)
            {
                mapCustomTextTriggers.SubVersion = null;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static GamePatch GetMinimumPatch(this MapCustomTextTriggers mapCustomTextTriggers)
        {
            return mapCustomTextTriggers.SubVersion.HasValue
                ? GamePatch.v1_31_0
                : mapCustomTextTriggers.FormatVersion == MapCustomTextTriggersFormatVersion.Tft
                    ? GamePatch.v1_07
                    : GamePatch.v1_00;
        }
    }
}