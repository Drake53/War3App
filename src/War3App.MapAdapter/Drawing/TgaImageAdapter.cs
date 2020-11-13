using System;
using System.IO;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Drawing
{
    public sealed class TgaImageAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "TGA Image";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), ".tga", StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            return new AdaptResult
            {
                Status = MapFileStatus.Compatible,
            };
        }
    }
}