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
    public sealed class DoodadObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Doodad)";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), MapDoodadObjectData.FileName.GetFileExtension(), StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var mapDoodadObjectData = MapDoodadObjectData.Parse(stream);

                try
                {
                    var knownIds = DoodadObjectDataProvider.GetRawcodes(targetPatch).ToHashSet();
                    var knownProperties = DoodadObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet();

                    var diagnostics = new List<string>();
                    var doodads = new List<ObjectModification>();
                    foreach (var doodad in mapDoodadObjectData.GetData())
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

                        if (doodad.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(doodad.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            doodads.Add(new ObjectModification(doodad.OldId, doodad.NewId, doodad.Where(property => knownProperties.Contains(property.Id)).ToArray()));
                        }
                        else
                        {
                            doodads.Add(doodad);
                        }
                    }

                    if (diagnostics.Count > 0)
                    {
                        var memoryStream = new MemoryStream();
                        MapDoodadObjectData.Serialize(new MapDoodadObjectData(doodads.ToArray()), memoryStream, true);

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