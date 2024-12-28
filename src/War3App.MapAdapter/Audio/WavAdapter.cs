using System.IO;

namespace War3App.MapAdapter.Audio
{
    public sealed class WavAdapter : IMapFileAdapter
    {
        private static readonly WavAdapter _instance = new();

        private WavAdapter()
        {
        }

        public static WavAdapter Instance => _instance;

        public string MapFileDescription => "Audio File (WAV)";

        public string DefaultFileName => "file.wav";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            return MapFileStatus.Compatible;
        }
    }
}