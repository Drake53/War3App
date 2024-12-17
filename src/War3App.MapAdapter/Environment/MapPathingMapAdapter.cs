using System;
using System.IO;
using System.Text;
using System.Text.Json;

using War3Net.Build.Common;
using War3Net.Build.Environment;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Environment
{
    public sealed class MapPathingMapAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Pathing Map";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            MapPathingMap pathingMap;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                pathingMap = reader.ReadMapPathingMap();
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
            var pathingMap = reader.ReadMapPathingMap();

            return JsonSerializer.Serialize(pathingMap, options);
        }
    }
}