using System;
using System.IO;
using System.Text;
using System.Text.Json;

using War3Net.Build.Common;
using War3Net.Build.Environment;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Environment
{
    public sealed class MapPreviewIconsAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Map Preview Icons";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            MapPreviewIcons mapPreviewIcons;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                mapPreviewIcons = reader.ReadMapPreviewIcons();
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
            var mapPreviewIcons = reader.ReadMapPreviewIcons();

            return JsonSerializer.Serialize(mapPreviewIcons, options);
        }
    }
}