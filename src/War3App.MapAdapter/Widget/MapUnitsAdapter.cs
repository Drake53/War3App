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
    public sealed class MapUnitsAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Units";

        public string DefaultFileName => MapUnits.FileName;

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            MapUnits mapUnits;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                mapUnits = reader.ReadMapUnits();
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            if (!mapUnits.Adapt(context, out var status))
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();

                using var writer = new BinaryWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(mapUnits);

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
            var mapUnits = reader.ReadMapUnits();

            return JsonSerializer.Serialize(mapUnits, options);
        }
    }
}