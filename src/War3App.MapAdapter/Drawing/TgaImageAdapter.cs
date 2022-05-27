﻿using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Drawing
{
    public sealed class TgaImageAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Image File (TGA)";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            return new AdaptResult
            {
                Status = MapFileStatus.Compatible,
            };
        }
    }
}