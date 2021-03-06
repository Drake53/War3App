﻿using System;
using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Script
{
    public sealed class MapTriggerStringsAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Trigger Strings";

        public bool IsTextFile => true;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
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