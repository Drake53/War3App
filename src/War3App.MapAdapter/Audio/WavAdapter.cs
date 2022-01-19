using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Audio
{
    public sealed class WavAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Audio File (WAV)";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            return new AdaptResult
            {
                Status = MapFileStatus.Compatible,
            };
        }
    }
}