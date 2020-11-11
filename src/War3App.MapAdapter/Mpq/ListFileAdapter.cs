using System;
using System.IO;

using War3Net.Build.Common;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.Mpq
{
    public sealed class ListFileAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "MPQ ListFile";

        public bool IsTextFile => true;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s, ListFile.Key, StringComparison.OrdinalIgnoreCase);
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