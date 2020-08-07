using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Object
{
    public sealed class DestructableObjectDataAdapter : IMapFileAdapter
    {
        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch)
        {
            try
            {
                // TODO: use MapDestructableObjectData as input
                // var mapDestructableObjectData = MapDestructableObjectData.Parse(stream);

                try
                {
                    var mapDestructableObjectDataStream = new MemoryStream();
                    if (DestructableObjectDataValidator.Downgrade(stream, mapDestructableObjectDataStream, targetPatch))
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = mapDestructableObjectDataStream,
                        };
                    }
                    else
                    {
                        mapDestructableObjectDataStream.Dispose();
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