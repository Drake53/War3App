using System;
using System.IO;
using System.Text;
using System.Text.Json;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Info;
using War3Net.Common.Providers;

namespace War3App.MapAdapter.Info
{
    public sealed class MapInfoAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Map Info";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            MapInfo mapInfo;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                mapInfo = reader.ReadMapInfo();
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            var status = mapInfo.Adapt(context);
            if (status != MapFileStatus.Adapted)
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();

                using var writer = new BinaryWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(mapInfo);

                return memoryStream;
            }
            catch (Exception e)
            {
                return context.ReportSerializeError(e);
            }
        }

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch, JsonSerializerOptions options)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            var mapInfo = reader.ReadMapInfo();

            return JsonSerializer.Serialize(mapInfo, options);
        }
    }
}