using System;
using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Mpq
{
    public sealed class AttributesAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "MPQ Attributes";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            return new AdaptResult
            {
                Status = MapFileStatus.Removed,
            };
        }

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch)
        {
            throw new NotSupportedException();
        }
    }
}