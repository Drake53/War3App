using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using War3Net.Build.Common;
using War3Net.Build.Object;
using War3Net.Common.Extensions;

namespace War3App.MapAdapter.Object
{
    public sealed class ItemObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Item)";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var mapItemObjectData = MapItemObjectData.Parse(stream);

                try
                {
                    var knownIds = ItemObjectDataProvider.GetRawcodes(targetPatch).ToHashSet();
                    var knownProperties = ItemObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet();

                    var diagnostics = new List<string>();
                    var items = new List<ObjectModification>();
                    foreach (var item in mapItemObjectData.GetData())
                    {
                        if (!knownIds.Contains(item.OldId))
                        {
                            diagnostics.Add($"Unknown base item: '{item.OldId.ToRawcode()}'");
                            continue;
                        }

                        if (knownIds.Contains(item.NewId))
                        {
                            diagnostics.Add($"Conflicting item: '{item.NewId.ToRawcode()}'");
                            continue;
                        }

                        if (item.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(item.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            items.Add(new ObjectModification(item.OldId, item.NewId, item.Where(property => knownProperties.Contains(property.Id)).ToArray()));
                        }
                        else
                        {
                            items.Add(item);
                        }
                    }

                    if (diagnostics.Count > 0)
                    {
                        var memoryStream = new MemoryStream();
                        MapItemObjectData.Serialize(new MapItemObjectData(items.ToArray()), memoryStream, true);

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