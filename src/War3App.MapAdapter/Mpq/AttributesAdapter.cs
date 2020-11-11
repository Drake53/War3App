using System;
using System.IO;

using War3Net.Build.Common;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.Mpq
{
    public sealed class AttributesAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "MPQ Attributes";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s, Attributes.Key, StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            return new AdaptResult
            {
                Status = MapFileStatus.Removed,
            };
        }
    }
}