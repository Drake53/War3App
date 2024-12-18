using System;
using System.IO;
using System.Text;

using War3Net.Build.Extensions;
using War3Net.Build.Script;
using War3Net.Common.Providers;

namespace War3App.MapAdapter.Script
{
    public sealed class MapCustomTextTriggersAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Custom Text Triggers";

        public string DefaultFileName => MapCustomTextTriggers.FileName;

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            MapCustomTextTriggers mapCustomTextTriggers;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                mapCustomTextTriggers = reader.ReadMapCustomTextTriggers(UTF8EncodingProvider.StrictUTF8);
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            var status = mapCustomTextTriggers.Adapt(context);
            if (status != MapFileStatus.Adapted)
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();

                using var writer = new BinaryWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(mapCustomTextTriggers, UTF8EncodingProvider.StrictUTF8);

                return memoryStream;
            }
            catch (Exception e)
            {
                return context.ReportSerializeError(e);
            }
        }
    }
}