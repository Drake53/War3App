using System;
using System.IO;
using System.Text;

using War3Net.Build.Common;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Audio
{
    public sealed class MapSoundsAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Sounds";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var mapSounds = reader.ReadMapSounds();
                if (mapSounds.GetMinimumPatch() <= targetPatch.Patch)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    if (mapSounds.TryDowngrade(targetPatch.Patch))
                    {
                        var newMapSoundsFileStream = new MemoryStream();
                        using var writer = new BinaryWriter(newMapSoundsFileStream, new UTF8Encoding(false, true), true);
                        writer.Write(mapSounds);

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

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var mapSounds = reader.ReadMapSounds();

                return System.Text.Json.JsonSerializer.Serialize(mapSounds);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}