using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

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

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            try
            {
                if (context.FileName is null)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.AdapterError,
                        Diagnostics = new[] { "Invalid context: FileName should be known to run this adapter." },
                    };
                }

                var isSkinFile = context.FileName.EndsWith($"Skin{ItemObjectData.FileExtension}");
                var isSkinFileSupported = context.TargetPatch.Patch >= GamePatch.v1_33_0;

                if (isSkinFile && !isSkinFileSupported)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Removed,
                    };
                }

                var itemDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.ItemDataPath);
                if (!File.Exists(itemDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = itemDataPath.GetFileNotFoundDiagnostics(),
                    };
                }
                
                var itemMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.ItemMetaDataPath);
                if (!File.Exists(itemMetaDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = itemMetaDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                ItemObjectData itemObjectData;
                try
                {
                    using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                    itemObjectData = reader.ReadItemObjectData();
                }
                catch (Exception e)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ParseError,
                        Diagnostics = new[] { e.Message },
                    };
                }

                var shouldDowngrade = itemObjectData.GetMinimumPatch() > context.TargetPatch.Patch;
                if (shouldDowngrade && !itemObjectData.TryDowngrade(context.TargetPatch.Patch))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Unadaptable,
                    };
                }

                var diagnostics = new List<string>();

                var mergedWithSkinObjectData = false;

                if (!isSkinFileSupported)
                {
                    var expectedSkinFileName = context.FileName.Replace(ItemObjectData.FileExtension, $"Skin{ItemObjectData.FileExtension}");

                    if (context.Archive.TryOpenFile(expectedSkinFileName, out var skinStream))
                    {
                        ItemObjectData? itemSkinObjectData;
                        try
                        {
                            using var reader = new BinaryReader(skinStream, Encoding.UTF8, true);
                            itemSkinObjectData = reader.ReadItemObjectData();
                        }
                        catch (Exception e)
                        {
                            diagnostics.Add(e.Message);
                            itemSkinObjectData = null;
                        }

                        if (itemSkinObjectData is not null &&
                            (itemSkinObjectData.BaseItems.Count > 0 ||
                             itemSkinObjectData.NewItems.Count > 0))
                        {
                            itemObjectData.MergeWith(itemSkinObjectData);

                            mergedWithSkinObjectData = true;
                        }
                    }
                }

                var knownIds = new HashSet<int>();
                knownIds.AddItemsFromSylkTable(itemDataPath, DataConstants.ItemDataKeyColumn);

                var knownProperties = new HashSet<int>();
                knownProperties.AddItemsFromSylkTable(itemMetaDataPath, DataConstants.MetaDataIdColumn);

                var baseItems = new List<SimpleObjectModification>();
                foreach (var item in itemObjectData.BaseItems)
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
                foreach (var item in itemObjectData.NewItems)
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

                if (shouldDowngrade || mergedWithSkinObjectData || diagnostics.Count > 0)
                {
                    var memoryStream = new MemoryStream();
                    using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                    writer.Write(new ItemObjectData(itemObjectData.FormatVersion)
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
            catch (NotSupportedException e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.Unadaptable,
                    Diagnostics = new[] { e.Message },
                };
            }
            catch (Exception e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.AdapterError,
                    Diagnostics = new[] { e.Message },
                };
            }
        }

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch)
        {
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                var itemObjectData = reader.ReadItemObjectData();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                return JsonSerializer.Serialize(itemObjectData, options);
            }
            catch (Exception e)
            {
                return $"{e.GetType().FullName}{System.Environment.NewLine}{e.Message}";
            }
        }
    }
}