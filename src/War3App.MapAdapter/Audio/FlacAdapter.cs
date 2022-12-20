﻿using System;
using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Audio
{
    public sealed class FlacAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Audio File (FLAC)";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            return new AdaptResult
            {
                Status = targetPatch.Patch >= GamePatch.v1_32_0 ? MapFileStatus.Compatible : MapFileStatus.Unadaptable,
            };
        }

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch)
        {
            throw new NotSupportedException();
        }
    }
}