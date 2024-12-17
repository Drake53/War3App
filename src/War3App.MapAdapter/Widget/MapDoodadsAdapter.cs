using System;
using System.IO;
using System.Text;
using System.Text.Json;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Widget;
using War3Net.Common.Providers;

namespace War3App.MapAdapter.Widget
{
    public sealed class MapDoodadsAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Doodads";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            MapDoodads mapDoodads;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                mapDoodads = reader.ReadMapDoodads();
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            var status = mapDoodads.Adapt(context);
            if (status != MapFileStatus.Adapted)
            {
                return status;
            }

            try
            {
                var newMapDoodadsFileStream = new MemoryStream();
                using var writer = new BinaryWriter(newMapDoodadsFileStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(mapDoodads);

                return newMapDoodadsFileStream;
            }
            catch (Exception e)
            {
                return context.ReportSerializeError(e);
            }
        }

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch, JsonSerializerOptions options)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            var mapDoodads = reader.ReadMapDoodads();

            return JsonSerializer.Serialize(mapDoodads, options);
        }
    }
}