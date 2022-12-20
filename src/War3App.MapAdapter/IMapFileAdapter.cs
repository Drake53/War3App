using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter
{
    public interface IMapFileAdapter
    {
        string MapFileDescription { get; }

        bool IsTextFile { get; }

        bool IsJsonSerializationSupported { get; }

        AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch);

        string SerializeFileToJson(Stream stream, GamePatch gamePatch);
    }
}