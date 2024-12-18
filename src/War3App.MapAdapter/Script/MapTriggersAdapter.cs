﻿using System;
using System.IO;
using System.Text;
using System.Text.Json;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Script;
using War3Net.Common.Providers;

namespace War3App.MapAdapter.Script
{
    public sealed class MapTriggersAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Triggers";

        public string DefaultFileName => MapTriggers.FileName;

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            MapTriggers mapTriggers;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                mapTriggers = reader.ReadMapTriggers();
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            if (!mapTriggers.Adapt(context, out var status))
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();

                using var writer = new BinaryWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(mapTriggers);

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
            var mapTriggers = reader.ReadMapTriggers();

            return JsonSerializer.Serialize(mapTriggers, options);
        }
    }
}