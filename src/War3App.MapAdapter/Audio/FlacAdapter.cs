using System;
using System.IO;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Audio
{
    public sealed class FlacAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Audio File (FLAC)";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), ".flac", StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            return new AdaptResult
            {
                Status = targetPatch >= GamePatch.v1_32_0 ? MapFileStatus.Compatible : MapFileStatus.Unadaptable,
            };
        }
    }
}