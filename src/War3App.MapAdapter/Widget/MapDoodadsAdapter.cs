using System;
using System.IO;
using System.Text;

using War3Net.Build.Common;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Widget
{
    public sealed class MapDoodadsAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Doodads";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var mapDoodads = reader.ReadMapDoodads();
                if (mapDoodads.GetMinimumPatch() <= targetPatch)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    if (mapDoodads.TryDowngrade(targetPatch))
                    {
                        var newMapDoodadsFileStream = new MemoryStream();
                        using var writer = new BinaryWriter(newMapDoodadsFileStream, new UTF8Encoding(false, true), true);
                        writer.Write(mapDoodads);

                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = newMapDoodadsFileStream,
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
            catch (NotSupportedException)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.Unadaptable,
                };
            }
            catch (Exception e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.ParseError,
                    Diagnostics = new[] { e.Message },
                };
            }
        }
    }
}