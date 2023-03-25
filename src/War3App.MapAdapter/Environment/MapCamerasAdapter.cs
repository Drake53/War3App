using System;
using System.IO;
using System.Text;
using System.Text.Json;

using War3Net.Build.Common;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Environment
{
    public sealed class MapCamerasAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Cameras";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var mapCameras = reader.ReadMapCameras();
                if (mapCameras.GetMinimumPatch() <= targetPatch.Patch)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    if (mapCameras.TryDowngrade(targetPatch.Patch))
                    {
                        var newMapCamerasFileStream = new MemoryStream();
                        using var writer = new BinaryWriter(newMapCamerasFileStream, new UTF8Encoding(false, true), true);
                        writer.Write(mapCameras);

                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = newMapCamerasFileStream,
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
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                var mapCameras = reader.ReadMapCameras();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                return JsonSerializer.Serialize(mapCameras, options);
            }
            catch (Exception e)
            {
                return $"{e.GetType().FullName}{System.Environment.NewLine}{e.Message}";
            }
        }
    }
}