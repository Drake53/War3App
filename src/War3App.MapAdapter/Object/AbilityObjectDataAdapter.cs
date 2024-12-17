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
    public sealed class AbilityObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Ability)";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            AbilityObjectData abilityObjectData;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                abilityObjectData = reader.ReadAbilityObjectData();
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            var status = abilityObjectData.Adapt(context);
            if (status != MapFileStatus.Adapted)
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(abilityObjectData);

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
            var abilityObjectData = reader.ReadAbilityObjectData();

            return JsonSerializer.Serialize(abilityObjectData, options);
        }
    }
}