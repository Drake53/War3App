using System;
using System.IO;
using System.Text.Json;

using War3Net.Build.Common;

namespace War3App.MapAdapter
{
    public interface IMapFileAdapter
    {
        string MapFileDescription { get; }

        bool IsTextFile { get; }

        bool IsJsonSerializationSupported { get; }

        AdaptResult AdaptFile(Stream stream, AdaptFileContext context);

        string SerializeFileToJson(Stream stream, GamePatch gamePatch, JsonSerializerOptions options) => IsJsonSerializationSupported
            ? throw new NotImplementedException()
            : throw new NotSupportedException();
    }
}