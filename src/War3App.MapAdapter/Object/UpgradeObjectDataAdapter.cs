using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Object
{
    public sealed class UpgradeObjectDataAdapter : IMapFileAdapter
    {
        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch)
        {
            try
            {
                // TODO: use MapUpgradeObjectData as input
                // var mapUpgradeObjectData = MapUpgradeObjectData.Parse(stream);

                try
                {
                    var mapUpgradeObjectDataStream = new MemoryStream();
                    if (UpgradeObjectDataValidator.Downgrade(stream, mapUpgradeObjectDataStream, targetPatch))
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = mapUpgradeObjectDataStream,
                        };
                    }
                    else
                    {
                        mapUpgradeObjectDataStream.Dispose();
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