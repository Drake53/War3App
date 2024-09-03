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
    public sealed class BuffObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Buff)";

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

                var isSkinFile = context.FileName.EndsWith($"Skin{BuffObjectData.FileExtension}");
                var isSkinFileSupported = context.TargetPatch.Patch >= GamePatch.v1_33_0;

                if (isSkinFile && !isSkinFileSupported)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Removed,
                    };
                }

                var buffDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.BuffDataPath);
                if (!File.Exists(buffDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = buffDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                var buffMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.BuffMetaDataPath);
                if (!File.Exists(buffMetaDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = buffMetaDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                BuffObjectData buffObjectData;
                try
                {
                    using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                    buffObjectData = reader.ReadBuffObjectData();
                }
                catch (Exception e)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ParseError,
                        Diagnostics = new[] { e.Message },
                    };
                }

                var shouldDowngrade = buffObjectData.GetMinimumPatch() > context.TargetPatch.Patch;
                if (shouldDowngrade && !buffObjectData.TryDowngrade(context.TargetPatch.Patch))
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
                    var expectedSkinFileName = context.FileName.Replace(BuffObjectData.FileExtension, $"Skin{BuffObjectData.FileExtension}");

                    if (context.Archive.TryOpenFile(expectedSkinFileName, out var skinStream))
                    {
                        BuffObjectData? buffSkinObjectData;
                        try
                        {
                            using var reader = new BinaryReader(skinStream, Encoding.UTF8, true);
                            buffSkinObjectData = reader.ReadBuffObjectData();
                        }
                        catch (Exception e)
                        {
                            diagnostics.Add(e.Message);
                            buffSkinObjectData = null;
                        }

                        if (buffSkinObjectData is not null &&
                            (buffSkinObjectData.BaseBuffs.Count > 0 ||
                             buffSkinObjectData.NewBuffs.Count > 0))
                        {
                            buffObjectData.MergeWith(buffSkinObjectData);

                            mergedWithSkinObjectData = true;
                        }
                    }
                }

                var knownIds = new HashSet<int>();
                knownIds.AddItemsFromSylkTable(buffDataPath, DataConstants.BuffDataKeyColumn);

                var knownProperties = new HashSet<int>();
                knownProperties.AddItemsFromSylkTable(buffMetaDataPath, DataConstants.MetaDataIdColumn);

                var baseBuffs = new List<SimpleObjectModification>();
                foreach (var buff in buffObjectData.BaseBuffs)
                {
                    if (!knownIds.Contains(buff.OldId))
                    {
                        diagnostics.Add($"Unknown base buff: '{buff.OldId.ToRawcode()}'");
                        continue;
                    }

                    if (buff.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                    {
                        diagnostics.AddRange(buff.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                        buff.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                    }

                    baseBuffs.Add(buff);
                }

                var newBuffs = new List<SimpleObjectModification>();
                foreach (var buff in buffObjectData.NewBuffs)
                {
                    if (!knownIds.Contains(buff.OldId))
                    {
                        diagnostics.Add($"Unknown base buff: '{buff.OldId.ToRawcode()}'");
                        continue;
                    }

                    if (knownIds.Contains(buff.NewId))
                    {
                        diagnostics.Add($"Conflicting buff: '{buff.NewId.ToRawcode()}'");
                        continue;
                    }

                    if (buff.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                    {
                        diagnostics.AddRange(buff.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                        buff.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                    }

                    newBuffs.Add(buff);
                }

                if (shouldDowngrade || mergedWithSkinObjectData || diagnostics.Count > 0)
                {
                    var memoryStream = new MemoryStream();
                    using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                    writer.Write(new BuffObjectData(buffObjectData.FormatVersion)
                    {
                        BaseBuffs = baseBuffs,
                        NewBuffs = newBuffs,
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
                var buffObjectData = reader.ReadBuffObjectData();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                return JsonSerializer.Serialize(buffObjectData, options);
            }
            catch (Exception e)
            {
                return $"{e.GetType().FullName}{System.Environment.NewLine}{e.Message}";
            }
        }
    }
}