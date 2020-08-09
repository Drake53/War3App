using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter
{
    public interface IMapFileAdapter
    {
        bool CanAdaptFile(string fileName);

        bool CanAdaptFile(Stream stream);

        AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch);
    }
}