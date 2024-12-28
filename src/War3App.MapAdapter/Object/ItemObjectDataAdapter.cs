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
    public sealed class ItemObjectDataAdapter : IMapFileAdapter
    {
        private static readonly ItemObjectDataAdapter _instance = new();

        private ItemObjectDataAdapter()
        {
        }

        public static ItemObjectDataAdapter Instance => _instance;

        public string MapFileDescription => "Object Data (Item)";

        public string DefaultFileName => ItemObjectData.MapFileName;

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            ItemObjectData itemObjectData;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                itemObjectData = reader.ReadItemObjectData();
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            if (!itemObjectData.Adapt(context, out var status))
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();

                using var writer = new BinaryWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(itemObjectData);

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
            var itemObjectData = reader.ReadItemObjectData();

            return JsonSerializer.Serialize(itemObjectData, options);
        }
    }
}