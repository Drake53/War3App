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
    public sealed class UnitObjectDataAdapter : IMapFileAdapter
    {
        private static readonly UnitObjectDataAdapter _instance = new();

        private UnitObjectDataAdapter()
        {
        }

        public static UnitObjectDataAdapter Instance => _instance;

        public string MapFileDescription => "Object Data (Unit)";

        public string DefaultFileName => UnitObjectData.MapFileName;

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            UnitObjectData unitObjectData;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                unitObjectData = reader.ReadUnitObjectData();
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            if (!unitObjectData.Adapt(context, out var status))
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();

                using var writer = new BinaryWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(unitObjectData);

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
            var unitObjectData = reader.ReadUnitObjectData();

            return JsonSerializer.Serialize(unitObjectData, options);
        }
    }
}