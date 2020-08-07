using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Object
{
    public sealed class ItemObjectDataAdapter : IMapFileAdapter
    {
        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch)
        {
            try
            {
                // TODO: use MapItemObjectData as input
                // var mapItemObjectData = MapItemObjectData.Parse(stream);

                try
                {
                    var mapItemObjectDataStream = new MemoryStream();
                    if (ItemObjectDataValidator.Downgrade(stream, mapItemObjectDataStream, targetPatch))
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = mapItemObjectDataStream,
                        };
                    }
                    else
                    {
                        mapItemObjectDataStream.Dispose();
                        return new AdaptResult
                        {
                            Status = MapFileStatus.Compatible,
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