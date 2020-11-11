using System;
using System.IO;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Modeling
{
    public sealed class TextModelAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Text Model";

        public bool IsTextFile => true;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), ".mdl", StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            // TODO
            return new AdaptResult
            {
                Status = MapFileStatus.Unknown,
            };
        }
    }
}