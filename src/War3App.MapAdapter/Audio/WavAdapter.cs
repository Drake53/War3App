using System.IO;

namespace War3App.MapAdapter.Audio
{
    public sealed class WavAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Audio File (WAV)";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            return MapFileStatus.Compatible;
        }
    }
}