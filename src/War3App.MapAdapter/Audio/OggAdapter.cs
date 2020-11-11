using System;
using System.IO;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Audio
{
    public sealed class OggAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Audio File (OGG)";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), ".ogg", StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            return new AdaptResult
            {
                Status = targetPatch >= GamePatch.v1_30_0 && targetPatch <= GamePatch.v1_30_4 ? MapFileStatus.Compatible : MapFileStatus.Unadaptable,
            };
        }
    }
}