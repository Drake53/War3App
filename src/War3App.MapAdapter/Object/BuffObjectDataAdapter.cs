using System;
using System.IO;
using System.Text;
using System.Text.Json;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Object;
using War3Net.Common.Providers;

namespace War3App.MapAdapter.Object
{
    public sealed class BuffObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Buff)";

        public string DefaultFileName => BuffObjectData.MapFileName;

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            BuffObjectData buffObjectData;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                buffObjectData = reader.ReadBuffObjectData();
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            if (!buffObjectData.Adapt(context, out var status))
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();

                using var writer = new BinaryWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(buffObjectData);

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
            var buffObjectData = reader.ReadBuffObjectData();

            return JsonSerializer.Serialize(buffObjectData, options);
        }
    }
}