using System;
using System.IO;
using System.Text;
using System.Text.Json;

using War3Net.Build.Common;
using War3Net.Build.Environment;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Environment
{
    public sealed class MapRegionsAdapter : IMapFileAdapter
    {
        private static readonly MapRegionsAdapter _instance = new();

        private MapRegionsAdapter()
        {
        }

        public static MapRegionsAdapter Instance => _instance;

        public string MapFileDescription => "Regions";

        public string DefaultFileName => MapRegions.FileName;

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            MapRegions mapRegions;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                mapRegions = reader.ReadMapRegions();
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
            var mapRegions = reader.ReadMapRegions();

            return JsonSerializer.Serialize(mapRegions, options);
        }
    }
}