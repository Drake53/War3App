using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Object;
using War3Net.Common.Extensions;

namespace War3App.MapAdapter.Object
{
    public sealed class ItemObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Item)";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var mapItemObjectData = reader.ReadMapItemObjectData();

                try
                {
                    var knownIds = new HashSet<int>();
                    knownIds.AddItemsFromSylkTable(Path.Combine(targetPatch.GameDataPath, PathConstants.ItemDataPath), DataConstants.ItemDataKeyColumn);

                    var knownProperties = new HashSet<int>();
                    knownProperties.AddItemsFromSylkTable(Path.Combine(targetPatch.GameDataPath, PathConstants.ItemMetaDataPath), DataConstants.MetaDataIdColumn);

                    var diagnostics = new List<string>();

                    var baseItems = new List<SimpleObjectModification>();
                    foreach (var item in mapItemObjectData.BaseItems)
                    {
                        if (!knownIds.Contains(item.OldId))
                        {
                            diagnostics.Add($"Unknown base item: '{item.OldId.ToRawcode()}'");
                            continue;
                        }

                        if (item.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(item.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            item.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                        }

                        baseItems.Add(item);
                    }

                    var newItems = new List<SimpleObjectModification>();
                    foreach (var item in mapItemObjectData.NewItems)
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

                        if (item.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(item.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            item.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                        }

                        newItems.Add(item);
                    }

                    if (diagnostics.Count > 0)
                    {
                        var memoryStream = new MemoryStream();
                        using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                        writer.Write(new MapItemObjectData(mapItemObjectData.FormatVersion)
                        {
                            BaseItems = baseItems,
                            NewItems = newItems,
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