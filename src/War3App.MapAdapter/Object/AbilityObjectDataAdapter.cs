using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Object
{
    public sealed class AbilityObjectDataAdapter : IMapFileAdapter
    {
        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch)
        {
            try
            {
                // TODO: use MapAbilityObjectData as input
                // var mapAbilityObjectData = MapAbilityObjectData.Parse(stream);

                try
                {
                    var mapAbilityObjectDataStream = new MemoryStream();
                    if (AbilityObjectDataValidator.Downgrade(stream, mapAbilityObjectDataStream, targetPatch))
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = mapAbilityObjectDataStream,
                        };
                    }
                    else
                    {
                        mapAbilityObjectDataStream.Dispose();
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