﻿using System;
using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Script
{
    public sealed class TriggerStringsAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Trigger Strings";

        public bool IsTextFile => true;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
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

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch)
        {
            throw new NotSupportedException();
        }
    }
}