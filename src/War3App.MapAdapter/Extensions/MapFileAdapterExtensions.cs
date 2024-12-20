﻿using System;
using System.IO;
using System.Text.Json;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Extensions
{
    public static class MapFileAdapterExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

        public static AdaptResult Run(this IMapFileAdapter adapter, Stream stream, AdaptFileContext context)
        {
            try
            {
                return adapter.AdaptFile(stream, context);
            }
            catch (Exception e)
            {
                return context.ReportAdapterError(e);
            }
        }

        public static string GetJson(this IMapFileAdapter adapter, Stream stream, GamePatch gamePatch)
        {
            try
            {
                return adapter.SerializeFileToJson(stream, gamePatch, _jsonSerializerOptions);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}