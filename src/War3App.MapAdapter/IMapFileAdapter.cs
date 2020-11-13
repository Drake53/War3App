using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter
{
    public interface IMapFileAdapter
    {
        public string MapFileDescription { get; }

        public bool IsTextFile { get; }

        AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch);
    }
}