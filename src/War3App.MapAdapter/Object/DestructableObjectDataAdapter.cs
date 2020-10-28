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
    public sealed class DestructableObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Destructable)";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), MapDestructableObjectData.FileName.GetFileExtension(), StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var mapDestructableObjectData = MapDestructableObjectData.Parse(stream);

                try
                {
                    var knownIds = DestructableObjectDataProvider.GetRawcodes(targetPatch).ToHashSet();
                    var knownProperties = DestructableObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet();

                    var diagnostics = new List<string>();
                    var destructables = new List<ObjectModification>();
                    foreach (var destructable in mapDestructableObjectData.GetData())
                    {
                        if (!knownIds.Contains(destructable.OldId))
                        {
                            diagnostics.Add($"Unknown base destructable: '{destructable.OldId.ToRawcode()}'");
                            continue;
                        }

                        if (knownIds.Contains(destructable.NewId))
                        {
                            diagnostics.Add($"Conflicting destructable: '{destructable.NewId.ToRawcode()}'");
                            continue;
                        }

                        if (destructable.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(destructable.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            destructables.Add(new ObjectModification(destructable.OldId, destructable.NewId, destructable.Where(property => knownProperties.Contains(property.Id)).ToArray()));
                        }
                        else
                        {
                            destructables.Add(destructable);
                        }
                    }

                    if (diagnostics.Count > 0)
                    {
                        var memoryStream = new MemoryStream();
                        MapDestructableObjectData.Serialize(new MapDestructableObjectData(destructables.ToArray()), memoryStream, true);

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