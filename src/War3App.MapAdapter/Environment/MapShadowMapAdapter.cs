using System;
using System.IO;
using System.Text;
using System.Text.Json;

using War3Net.Build.Common;
using War3Net.Build.Environment;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Environment
{
    public sealed class MapShadowMapAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Shadow Map";

        public string DefaultFileName => MapShadowMap.FileName;

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            MapShadowMap shadowMap;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                shadowMap = reader.ReadMapShadowMap();
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
            var shadowMap = reader.ReadMapShadowMap();

            return JsonSerializer.Serialize(shadowMap, options);
        }
    }
}