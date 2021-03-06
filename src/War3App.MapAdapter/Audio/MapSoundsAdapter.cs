﻿using System;
using System.IO;

using War3Net.Build.Audio;
using War3Net.Build.Common;

namespace War3App.MapAdapter.Audio
{
    public sealed class MapSoundsAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Sounds";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var mapSounds = MapSounds.Parse(stream);
                if (mapSounds.GetMinimumPatch() <= targetPatch)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    if (mapSounds.TryDowngrade(targetPatch))
                    {
                        var newMapSoundsFileStream = new MemoryStream();
                        mapSounds.SerializeTo(newMapSoundsFileStream, true);

                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = newMapSoundsFileStream,
                        };
                    }
                    else
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.Unadaptable,
                        };
                    }
                }
                catch
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.AdapterError,
                    };
                }
            }
            catch (NotSupportedException)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.Unadaptable,
                };
            }
            catch (Exception e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.ParseError,
                    Diagnostics = new[] { e.Message },
                };
            }
        }
    }
}