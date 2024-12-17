using System;

using War3Net.Build.Audio;
using War3Net.Build.Common;

namespace War3App.MapAdapter.Audio
{
    public static class MapSoundsExtensions
    {
        public static MapFileStatus Adapt(this MapSounds mapSounds, AdaptFileContext context)
        {
            if (mapSounds.GetMinimumPatch() <= context.TargetPatch.Patch)
            {
                return MapFileStatus.Compatible;
            }

            return mapSounds.TryDowngrade(context.TargetPatch.Patch)
                ? MapFileStatus.Adapted
                : MapFileStatus.Incompatible;
        }

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
                case MapSoundsFormatVersion.v3:
                    mapSounds.FormatVersion = MapSoundsFormatVersion.v2;
                    break;

                case MapSoundsFormatVersion.v2:
                    foreach (var sound in mapSounds.Sounds)
                    {
                        // TODO: warn/error if it's a .flac file
                        sound.SoundName = null;
                    }

                    mapSounds.FormatVersion = MapSoundsFormatVersion.v1;
                    break;

                default: break;
            }
        }

        public static GamePatch GetMinimumPatch(this MapSounds mapSounds)
        {
            return mapSounds.FormatVersion switch
            {
                MapSoundsFormatVersion.v1 => GamePatch.v1_00,
                MapSoundsFormatVersion.v2 => GamePatch.v1_32_0,
                MapSoundsFormatVersion.v3 => GamePatch.v1_32_6,
            };
        }
    }
}