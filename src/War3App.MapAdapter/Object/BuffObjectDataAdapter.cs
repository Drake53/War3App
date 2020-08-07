using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Object
{
    public sealed class BuffObjectDataAdapter : IMapFileAdapter
    {
        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch)
        {
            try
            {
                // TODO: use MapBuffObjectData as input
                // var mapBuffObjectData = MapBuffObjectData.Parse(stream);

                try
                {
                    var mapBuffObjectDataStream = new MemoryStream();
                    if (BuffObjectDataValidator.Downgrade(stream, mapBuffObjectDataStream, targetPatch))
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = mapBuffObjectDataStream,
                        };
                    }
                    else
                    {
                        mapBuffObjectDataStream.Dispose();
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