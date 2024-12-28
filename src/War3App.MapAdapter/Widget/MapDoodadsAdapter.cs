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
        private static readonly MapDoodadsAdapter _instance = new();

        private MapDoodadsAdapter()
        {
        }

        public static MapDoodadsAdapter Instance => _instance;

        public string MapFileDescription => "Doodads";

        public string DefaultFileName => MapDoodads.FileName;

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

            if (!mapDoodads.Adapt(context, out var status))
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();

                using var writer = new BinaryWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(mapDoodads);

                return AdaptResult.Create(memoryStream, status);
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