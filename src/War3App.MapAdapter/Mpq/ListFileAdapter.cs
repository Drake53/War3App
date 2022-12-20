using System;
using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Mpq
{
    public sealed class ListFileAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "MPQ ListFile";

        public bool IsTextFile => true;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            return new AdaptResult
            {
                Status = MapFileStatus.Compatible,
            };
        }

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch)
        {
            throw new NotSupportedException();
        }
    }
}