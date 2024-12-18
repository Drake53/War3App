using System.IO;

namespace War3App.MapAdapter.Audio
{
    public sealed class Mp3Adapter : IMapFileAdapter
    {
        public string MapFileDescription => "Audio File (MP3)";

        public string DefaultFileName => "file.mp3";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            return MapFileStatus.Compatible;
        }
    }
}