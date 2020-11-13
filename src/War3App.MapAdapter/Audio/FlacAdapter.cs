using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Audio
{
    public sealed class FlacAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Audio File (FLAC)";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            return new AdaptResult
            {
                Status = targetPatch >= GamePatch.v1_32_0 ? MapFileStatus.Compatible : MapFileStatus.Unadaptable,
            };
        }
    }
}