using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Object;
using War3Net.Common.Extensions;

namespace War3App.MapAdapter.Object
{
    public sealed class UnitObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Unit)";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), MapUnitObjectData.FileName.GetFileExtension(), StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var mapUnitObjectData = MapUnitObjectData.Parse(stream);

                try
                {
                    var knownIds = UnitObjectDataProvider.GetRawcodes(targetPatch).ToHashSet();
                    var knownProperties = UnitObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet();

                    var diagnostics = new List<string>();
                    var units = new List<ObjectModification>();
                    foreach (var unit in mapUnitObjectData.GetData())
                    {
                        if (!knownIds.Contains(unit.OldId))
                        {
                            diagnostics.Add($"Unknown base unit: '{unit.OldId.ToRawcode()}'");
                            continue;
                        }

                        if (knownIds.Contains(unit.NewId))
                        {
                            diagnostics.Add($"Conflicting unit: '{unit.NewId.ToRawcode()}'");
                            continue;
                        }

                        if (unit.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(unit.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            units.Add(new ObjectModification(unit.OldId, unit.NewId, unit.Where(property => knownProperties.Contains(property.Id)).ToArray()));
                        }
                        else
                        {
                            units.Add(unit);
                        }
                    }

                    if (diagnostics.Count > 0)
                    {
                        var memoryStream = new MemoryStream();
                        MapUnitObjectData.Serialize(new MapUnitObjectData(units.ToArray()), memoryStream, true);

                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            Diagnostics = diagnostics.ToArray(),
                            AdaptedFileStream = memoryStream,
                        };
                    }
                    else
                    {
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
            catch (NotSupportedException)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.Unadaptable,
                };
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