using System;
using System.IO;
using System.Text;
using System.Text.Json;

using War3Net.Build.Common;
using War3Net.Build.Environment;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Environment
{
    public sealed class MapEnvironmentAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Environment";

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

            return MapFileStatus.Compatible;
        }

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch, JsonSerializerOptions options)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            var mapEnvironment = reader.ReadMapEnvironment();

            return JsonSerializer.Serialize(mapEnvironment, options);
        }
    }
}