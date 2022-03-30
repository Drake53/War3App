﻿using System;
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
    public sealed class DestructableObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Destructable)";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var destructableDataPath = Path.Combine(targetPatch.GameDataPath, PathConstants.DestructableDataPath);
                if (!File.Exists(destructableDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = destructableDataPath.GetFileNotFoundDiagnostics(),
                    };
                }
                
                var destructableMetaDataPath = Path.Combine(targetPatch.GameDataPath, PathConstants.DestructableMetaDataPath);
                if (!File.Exists(destructableMetaDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = destructableMetaDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                using var reader = new BinaryReader(stream);
                var mapDestructableObjectData = reader.ReadMapDestructableObjectData();

                try
                {
                    var knownIds = new HashSet<int>();
                    knownIds.AddItemsFromSylkTable(destructableDataPath, DataConstants.DestructableDataKeyColumn);

                    var knownProperties = new HashSet<int>();
                    knownProperties.AddItemsFromSylkTable(destructableMetaDataPath, DataConstants.MetaDataIdColumn);

                    var diagnostics = new List<string>();

                    var baseDestructables = new List<SimpleObjectModification>();
                    foreach (var destructable in mapDestructableObjectData.BaseDestructables)
                    {
                        if (!knownIds.Contains(destructable.OldId))
                        {
                            diagnostics.Add($"Unknown base destructable: '{destructable.OldId.ToRawcode()}'");
                            continue;
                        }

                        if (destructable.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(destructable.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            destructable.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                        }

                        baseDestructables.Add(destructable);
                    }

                    var newDestructables = new List<SimpleObjectModification>();
                    foreach (var destructable in mapDestructableObjectData.NewDestructables)
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

                        if (destructable.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(destructable.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            destructable.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                        }

                        newDestructables.Add(destructable);
                    }

                    if (diagnostics.Count > 0)
                    {
                        var memoryStream = new MemoryStream();
                        using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                        writer.Write(new MapDestructableObjectData(mapDestructableObjectData.FormatVersion)
                        {
                            BaseDestructables = baseDestructables,
                            NewDestructables = newDestructables,
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