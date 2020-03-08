using System;

using War3Net.Build.Common;
using War3Net.Build.Info;

namespace War3App.MapDowngrader
{
    public static class MapInfoDowngrader
    {
        public static bool TryDowngrade(this MapInfo mapInfo, GamePatch targetPatch)
        {
            try
            {
                while (mapInfo.GetMinimumPatch() > targetPatch)
                {
                    mapInfo.DowngradeOnce();
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

        public static void DowngradeOnce(this MapInfo mapInfo)
        {
            switch (mapInfo.FormatVersion)
            {
                case MapInfoFormatVersion.Reforged:
                    mapInfo.MapFlags &= ~(MapFlags.AccurateProbabilityForCalculations | MapFlags.CustomAbilitySkin);
                    mapInfo.EditorVersion = 6072;
                    mapInfo.GameVersion = new Version(1, 31, 1, 12173);
                    var playerData = new PlayerData[mapInfo.PlayerDataCount];
                    for (var i = 0; i < mapInfo.PlayerDataCount; i++)
                    {
                        playerData[i] = PlayerData.Create(mapInfo.GetPlayerData(i), false);
                    }

                    mapInfo.SetPlayerData(playerData);

                    mapInfo.FormatVersion = MapInfoFormatVersion.Lua;
                    break;

                case MapInfoFormatVersion.Lua:
                    if (mapInfo.ScriptLanguage == ScriptLanguage.Lua)
                    {
                        throw new NotSupportedException();
                    }

                    mapInfo.GameVersion = null;

                    mapInfo.FormatVersion = MapInfoFormatVersion.Tft;
                    break;

                case MapInfoFormatVersion.Tft:
                    throw new NotImplementedException();

                default: break;
            }
        }

        public static GamePatch GetMinimumPatch(this MapInfo mapInfo)
        {
            return mapInfo.FormatVersion switch
            {
                MapInfoFormatVersion.RoC => GamePatch.v1_00,
                MapInfoFormatVersion.Tft => GamePatch.v1_07,
                MapInfoFormatVersion.Lua => GamePatch.v1_31_0,
                MapInfoFormatVersion.Reforged => GamePatch.v1_32_0,
            };
        }
    }
}