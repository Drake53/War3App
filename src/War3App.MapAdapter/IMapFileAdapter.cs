using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter
{
    public interface IMapFileAdapter
    {
        AdaptResult AdaptFile(Stream stream, GamePatch targetPatch);
    }
}