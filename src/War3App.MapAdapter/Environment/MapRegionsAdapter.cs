﻿using System;
using System.IO;

using War3Net.Build.Common;
using War3Net.Build.Environment;

namespace War3App.MapAdapter.Environment
{
    public sealed class MapRegionsAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Regions";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var mapRegions = MapRegions.Parse(stream);
                return new AdaptResult
                {
                    Status = MapFileStatus.Compatible,
                };
            }
            catch (NotSupportedException)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.Unadaptable,
                };
            }
            catch (Exception e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.ParseError,
                    Diagnostics = new[] { e.Message },
                };
            }
        }
    }
}