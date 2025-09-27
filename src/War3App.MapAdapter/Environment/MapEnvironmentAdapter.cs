using System;
using System.IO;
using System.Text;
using System.Text.Json;

using War3Net.Build.Common;
using War3Net.Build.Environment;
using War3Net.Build.Extensions;
using War3Net.Common.Providers;

namespace War3App.MapAdapter.Environment
{
    public sealed class MapEnvironmentAdapter : IMapFileAdapter
    {
        private static readonly MapEnvironmentAdapter _instance = new();

        private MapEnvironmentAdapter()
        {
        }

        public static MapEnvironmentAdapter Instance => _instance;

        public string MapFileDescription => "Environment";

        public string DefaultFileName => MapEnvironment.FileName;

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            MapEnvironment mapEnvironment;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                mapEnvironment = reader.ReadMapEnvironment();
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            if (!mapEnvironment.Adapt(context, out var status))
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();

                using var writer = new BinaryWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(mapEnvironment);

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
            var mapEnvironment = reader.ReadMapEnvironment();

            return JsonSerializer.Serialize(mapEnvironment, options);
        }
    }
}