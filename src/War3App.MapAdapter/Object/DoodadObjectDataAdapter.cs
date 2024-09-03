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
    public sealed class DoodadObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Doodad)";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            try
            {
                var doodadDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.DoodadDataPath);
                if (!File.Exists(doodadDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = doodadDataPath.GetFileNotFoundDiagnostics(),
                    };
                }
                
                var doodadMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.DoodadMetaDataPath);
                if (!File.Exists(doodadMetaDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = doodadMetaDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                DoodadObjectData doodadObjectData;
                try
                {
                    using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                    doodadObjectData = reader.ReadDoodadObjectData();
                }
                catch (Exception e)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ParseError,
                        Diagnostics = new[] { e.Message },
                    };
                }

                var shouldDowngrade = doodadObjectData.GetMinimumPatch() > context.TargetPatch.Patch;
                if (shouldDowngrade && !doodadObjectData.TryDowngrade(context.TargetPatch.Patch))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Unadaptable,
                    };
                }

                var knownIds = new HashSet<int>();
                knownIds.AddItemsFromSylkTable(doodadDataPath, DataConstants.DoodadDataKeyColumn);

                var knownProperties = new HashSet<int>();
                knownProperties.AddItemsFromSylkTable(doodadMetaDataPath, DataConstants.MetaDataIdColumn);

                var diagnostics = new List<string>();

                var baseDoodads = new List<VariationObjectModification>();
                foreach (var doodad in doodadObjectData.BaseDoodads)
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
                foreach (var doodad in doodadObjectData.NewDoodads)
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

                if (shouldDowngrade || diagnostics.Count > 0)
                {
                    var memoryStream = new MemoryStream();
                    using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                    writer.Write(new DoodadObjectData(doodadObjectData.FormatVersion)
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
                var doodadObjectData = reader.ReadDoodadObjectData();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                return JsonSerializer.Serialize(doodadObjectData, options);
            }
            catch (Exception e)
            {
                return $"{e.GetType().FullName}{System.Environment.NewLine}{e.Message}";
            }
        }
    }
}