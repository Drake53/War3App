using System;
using System.IO;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Info;
using War3Net.Build.Providers;

namespace War3App.MapAdapter.Info
{
    public static class MapInfoExtensions
    {
        public static GamePatch? GetOriginGamePatch(this MapInfo mapInfo)
        {
            if (mapInfo.GameVersion != null)
            {
                if (mapInfo.GameVersion.Major == 1)
                {
                    if (mapInfo.GameVersion.Minor == 31)
                    {
                        if (mapInfo.GameVersion.Build == 0)
                        {
                            return GamePatch.v1_31_0;
                        }
                        else if (mapInfo.GameVersion.Build == 1)
                        {
                            return GamePatch.v1_31_1;
                        }
                        else
                        {
                            throw new InvalidDataException();
                        }
                    }
                    else if (mapInfo.GameVersion.Minor == 32)
                    {
                        if (mapInfo.GameVersion.Build == 0)
                        {
                            return GamePatch.v1_32_0;
                        }
                        else if (mapInfo.GameVersion.Build == 1)
                        {
                            return GamePatch.v1_32_1;
                        }
                        else if (mapInfo.GameVersion.Build == 2)
                        {
                            return GamePatch.v1_32_2;
                        }
                        else if (mapInfo.GameVersion.Build == 3)
                        {
                            return GamePatch.v1_32_3;
                        }
                        else if (mapInfo.GameVersion.Build == 4)
                        {
                            return GamePatch.v1_32_4;
                        }
                        else if (mapInfo.GameVersion.Build == 5)
                        {
                            return GamePatch.v1_32_5;
                        }
                        else if (mapInfo.GameVersion.Build == 6)
                        {
                            return GamePatch.v1_32_6;
                        }
                        else if (mapInfo.GameVersion.Build == 7)
                        {
                            return GamePatch.v1_32_7;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            // else if (mapInfo.ScriptLanguage == ScriptLanguage.Lua)
            // {
            //     return GamePatch.v1_31_0;
            // }
            else
            {
                if (mapInfo.EditorVersion >= 6090 && mapInfo.EditorVersion < 7000)
                {
                    return GamePatch.v1_32_0;
                }
                else if (mapInfo.EditorVersion == 6072)
                {
                    return GamePatch.v1_31_0;
                }
                else if (mapInfo.EditorVersion == 6061)
                {
                    return GamePatch.v1_30_0;
                }
                else if (mapInfo.EditorVersion == 6060)
                {
                    return GamePatch.v1_29_0;
                }
                else if (mapInfo.EditorVersion == 6059)
                {
                    return GamePatch.v1_28_5;
                }
                // todo: more patchs
                else if (mapInfo.EditorVersion == 6052)
                {
                    return GamePatch.v1_23; // todo: check correctness
                }
                else
                {
                    throw new InvalidDataException();
                }
            }
        }

        public static bool TryDowngrade(this MapInfo mapInfo, GamePatch targetPatch)
        {
            try
            {
                while (mapInfo.GetMinimumPatch() > targetPatch)
                {
                    mapInfo.DowngradeOnce();
                }

                mapInfo.EditorVersion = targetPatch.GetEditorVersion();
                if (mapInfo.FormatVersion >= MapInfoFormatVersion.Lua)
                {
                    mapInfo.GameVersion = GamePatchVersionProvider.GetGameVersion(targetPatch);
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
                    throw new NotSupportedException();

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this MapInfo mapInfo)
        {
            var minimumBySlotCounts = mapInfo.PlayerDataCount > 12 || mapInfo.ForceDataCount > 12 ? GamePatch.v1_29_0 : GamePatch.v1_00;
            var minimumByFormatVersion = mapInfo.FormatVersion switch
            {
                MapInfoFormatVersion.RoC => GamePatch.v1_00,
                MapInfoFormatVersion.Tft => GamePatch.v1_07,
                MapInfoFormatVersion.Lua => GamePatch.v1_31_0,
                MapInfoFormatVersion.Reforged => GamePatch.v1_32_0,

                MapInfoFormatVersion.v8 => GamePatch.v1_00,
                MapInfoFormatVersion.v10 => GamePatch.v1_00,
                MapInfoFormatVersion.v11 => GamePatch.v1_00,
                MapInfoFormatVersion.v15 => GamePatch.v1_00,
                MapInfoFormatVersion.v23 => GamePatch.v1_07,
                MapInfoFormatVersion.v24 => GamePatch.v1_07,
                MapInfoFormatVersion.v26 => GamePatch.v1_31_0,
                MapInfoFormatVersion.v27 => GamePatch.v1_31_0,
            };

            return minimumBySlotCounts > minimumByFormatVersion ? minimumBySlotCounts : minimumByFormatVersion;
        }
    }
}