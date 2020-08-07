using System;

using War3Net.Build.Audio;
using War3Net.Build.Common;

namespace War3App.MapAdapter.Audio
{
    public static class MapSoundsExtensions
    {
        public static bool TryDowngrade(this MapSounds mapSounds, GamePatch targetPatch)
        {
            try
            {
                while (mapSounds.GetMinimumPatch() > targetPatch)
                {
                    mapSounds.DowngradeOnce();
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

        public static void DowngradeOnce(this MapSounds mapSounds)
        {
            switch (mapSounds.FormatVersion)
            {
                case MapSoundsFormatVersion.ReforgedV3:
                    mapSounds.FormatVersion = MapSoundsFormatVersion.Reforged;
                    break;

                case MapSoundsFormatVersion.Reforged:
                    foreach (var sound in mapSounds)
                    {
                        sound.SoundName = null;
                    }

                    mapSounds.FormatVersion = MapSoundsFormatVersion.Normal;
                    break;

                default: break;
            }
        }

        public static GamePatch GetMinimumPatch(this MapSounds mapSounds)
        {
            return mapSounds.FormatVersion switch
            {
                MapSoundsFormatVersion.Normal => GamePatch.v1_00,
                MapSoundsFormatVersion.Reforged => GamePatch.v1_32_0,
                MapSoundsFormatVersion.ReforgedV3 => GamePatch.v1_32_5, // todo; verify correctness
            };
        }
    }
}