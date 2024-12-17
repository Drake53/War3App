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
    public sealed class DestructableObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Destructable)";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            DestructableObjectData destructableObjectData;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                destructableObjectData = reader.ReadDestructableObjectData();
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            var status = destructableObjectData.Adapt(context);
            if (status != MapFileStatus.Adapted)
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();

                using var writer = new BinaryWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(destructableObjectData);

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
            var destructableObjectData = reader.ReadDestructableObjectData();

            return JsonSerializer.Serialize(destructableObjectData, options);
        }
    }
}