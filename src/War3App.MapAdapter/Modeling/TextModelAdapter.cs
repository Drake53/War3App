using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Modeling
{
    public sealed class TextModelAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Text Model";

        public bool IsTextFile => true;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            // TODO
            return new AdaptResult
            {
                Status = MapFileStatus.Unknown,
            };
        }
    }
}