using System;

using War3Net.Build.Common;
using War3Net.Build.Script;

namespace War3App.MapAdapter.Script
{
    public static class MapTriggersExtensions
    {
        public static bool TryDowngrade(this MapTriggers mapTriggers, GamePatch targetPatch)
        {
            try
            {
                while (mapTriggers.GetMinimumPatch() > targetPatch)
                {
                    mapTriggers.DowngradeOnce();
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

        public static void DowngradeOnce(this MapTriggers mapTriggers)
        {
            if (mapTriggers.SubVersion.HasValue)
            {
                mapTriggers.SubVersion = null;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static GamePatch GetMinimumPatch(this MapTriggers mapTriggers)
        {
            return mapTriggers.SubVersion.HasValue
                ? GamePatch.v1_31_0
                : mapTriggers.FormatVersion == MapTriggersFormatVersion.Tft
                    ? GamePatch.v1_07
                    : GamePatch.v1_00;
        }
    }
}