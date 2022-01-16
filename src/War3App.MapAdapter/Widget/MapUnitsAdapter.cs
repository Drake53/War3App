using System;
using System.IO;
using System.Text;

using War3Net.Build.Common;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Widget
{
    public sealed class MapUnitsAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Units";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var mapUnits = reader.ReadMapUnits();
                if (mapUnits.GetMinimumPatch() <= targetPatch)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    if (mapUnits.TryDowngrade(targetPatch))
                    {
                        var newMapUnitsFileStream = new MemoryStream();
                        using var writer = new BinaryWriter(newMapUnitsFileStream, new UTF8Encoding(false, true), true);
                        writer.Write(mapUnits);

                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = newMapUnitsFileStream,
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