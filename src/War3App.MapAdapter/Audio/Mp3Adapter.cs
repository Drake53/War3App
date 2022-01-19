using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Audio
{
    public sealed class Mp3Adapter : IMapFileAdapter
    {
        public string MapFileDescription => "Audio File (MP3)";

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