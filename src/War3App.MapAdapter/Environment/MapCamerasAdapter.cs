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
    public sealed class MapCamerasAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Cameras";

        public string DefaultFileName => MapCameras.FileName;

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            MapCameras mapCameras;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                mapCameras = reader.ReadMapCameras();
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            var status = mapCameras.Adapt(context);
            if (status != MapFileStatus.Adapted)
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();

                using var writer = new BinaryWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(mapCameras);

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
            var mapCameras = reader.ReadMapCameras();

            return JsonSerializer.Serialize(mapCameras, options);
        }
    }
}