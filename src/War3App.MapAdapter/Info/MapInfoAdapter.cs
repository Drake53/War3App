using System;
using System.IO;
using System.Text;
using System.Text.Json;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Info;

namespace War3App.MapAdapter.Info
{
    public sealed class MapInfoAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Map Info";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            MapInfo mapInfo;
            try
            {
                using var reader = new BinaryReader(stream);
                mapInfo = reader.ReadMapInfo();
            }
            catch (Exception e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.ParseError,
                    Diagnostics = new[] { e.Message },
                };
            }

            try
            {
                if (mapInfo.GetMinimumPatch() <= targetPatch.Patch)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                if (mapInfo.TryDowngrade(targetPatch.Patch))
                {
                    var newMapInfoFileStream = new MemoryStream();
                    using var writer = new BinaryWriter(newMapInfoFileStream, new UTF8Encoding(false, true), true);
                    writer.Write(mapInfo);

                    return new AdaptResult
                    {
                        Status = MapFileStatus.Adapted,
                        AdaptedFileStream = newMapInfoFileStream,
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
            catch (NotSupportedException e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.Unadaptable,
                    Diagnostics = new[] { e.Message },
                };
            }
            catch (Exception e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.AdapterError,
                    Diagnostics = new[] { e.Message },
                };
            }
        }

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch)
        {
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                var mapInfo = reader.ReadMapInfo();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                return JsonSerializer.Serialize(mapInfo, options);
            }
            catch (Exception e)
            {
                return $"{e.GetType().FullName}{System.Environment.NewLine}{e.Message}";
            }
        }
    }
}