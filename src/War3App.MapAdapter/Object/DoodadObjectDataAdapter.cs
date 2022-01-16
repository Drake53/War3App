using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Object;
using War3Net.Common.Extensions;

namespace War3App.MapAdapter.Object
{
    public sealed class DoodadObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Doodad)";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var mapDoodadObjectData = reader.ReadMapDoodadObjectData();

                try
                {
                    var knownIds = DoodadObjectDataProvider.GetRawcodes(targetPatch).ToHashSet();
                    var knownProperties = DoodadObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet();

                    var diagnostics = new List<string>();

                    var baseDoodads = new List<VariationObjectModification>();
                    foreach (var doodad in mapDoodadObjectData.BaseDoodads)
                    {
                        if (!knownIds.Contains(doodad.OldId))
                        {
                            diagnostics.Add($"Unknown base doodad: '{doodad.OldId.ToRawcode()}'");
                            continue;
                        }

                        if (doodad.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(doodad.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            doodad.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                        }

                        baseDoodads.Add(doodad);
                    }

                    var newDoodads = new List<VariationObjectModification>();
                    foreach (var doodad in mapDoodadObjectData.NewDoodads)
                    {
                        if (!knownIds.Contains(doodad.OldId))
                        {
                            diagnostics.Add($"Unknown base doodad: '{doodad.OldId.ToRawcode()}'");
                            continue;
                        }

                        if (knownIds.Contains(doodad.NewId))
                        {
                            diagnostics.Add($"Conflicting doodad: '{doodad.NewId.ToRawcode()}'");
                            continue;
                        }

                        if (doodad.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(doodad.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            doodad.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                        }

                        newDoodads.Add(doodad);
                    }

                    if (diagnostics.Count > 0)
                    {
                        var memoryStream = new MemoryStream();
                        using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                        writer.Write(new MapDoodadObjectData(mapDoodadObjectData.FormatVersion)
                        {
                            BaseDoodads = baseDoodads,
                            NewDoodads = newDoodads,
                        });

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