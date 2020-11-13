using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Mpq
{
    public sealed class AttributesAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "MPQ Attributes";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            return new AdaptResult
            {
                Status = MapFileStatus.Removed,
            };
        }
    }
}