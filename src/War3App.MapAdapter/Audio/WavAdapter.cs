using System;
using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Audio
{
    public sealed class WavAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Audio File (WAV)";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            return new AdaptResult
            {
                Status = MapFileStatus.Compatible,
            };
        }

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch)
        {
            throw new NotSupportedException();
        }
    }
}