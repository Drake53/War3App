using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Drawing
{
    public sealed class TgaImageAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "TGA Image";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            return new AdaptResult
            {
                Status = MapFileStatus.Compatible,
            };
        }
    }
}