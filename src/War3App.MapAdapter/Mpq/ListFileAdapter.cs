using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Mpq
{
    public sealed class ListFileAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "MPQ ListFile";

        public bool IsTextFile => true;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            return new AdaptResult
            {
                Status = MapFileStatus.Compatible,
            };
        }
    }
}