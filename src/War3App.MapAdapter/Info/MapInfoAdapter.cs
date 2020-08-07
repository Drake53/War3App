using System.IO;

using War3Net.Build.Common;
using War3Net.Build.Info;

namespace War3App.MapAdapter.Info
{
    public sealed class MapInfoAdapter : IMapFileAdapter
    {
        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch)
        {
            try
            {
                var mapInfo = MapInfo.Parse(stream);
                if (mapInfo.GetMinimumPatch() <= targetPatch)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    if (mapInfo.TryDowngrade(targetPatch))
                    {
                        var newMapInfoFileStream = new MemoryStream();
                        mapInfo.SerializeTo(newMapInfoFileStream, true);

                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = newMapInfoFileStream,
                        };
                    }
                    else
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.Unadaptable,
                        };
                    }
                }
                catch
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.AdapterError,
                    };
                }
            }
            catch
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.ParseError,
                };
            }
        }
    }
}