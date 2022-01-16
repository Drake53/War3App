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
    public sealed class AbilityObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Ability)";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var mapAbilityObjectData = reader.ReadMapAbilityObjectData();

                try
                {
                    var knownIds = AbilityObjectDataProvider.GetRawcodes(targetPatch).ToHashSet();
                    var knownProperties = AbilityObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet();

                    var diagnostics = new List<string>();

                    var baseAbilities = new List<LevelObjectModification>();
                    foreach (var ability in mapAbilityObjectData.BaseAbilities)
                    {
                        if (!knownIds.Contains(ability.OldId))
                        {
                            diagnostics.Add($"Unknown base ability: '{ability.OldId.ToRawcode()}'");
                            continue;
                        }

                        if (ability.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(ability.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            ability.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                        }

                        baseAbilities.Add(ability);
                    }

                    var newAbilities = new List<LevelObjectModification>();
                    foreach (var ability in mapAbilityObjectData.NewAbilities)
                    {
                        if (!knownIds.Contains(ability.OldId))
                        {
                            diagnostics.Add($"Unknown base ability: '{ability.OldId.ToRawcode()}'");
                            continue;
                        }

                        if (knownIds.Contains(ability.NewId))
                        {
                            diagnostics.Add($"Conflicting ability: '{ability.NewId.ToRawcode()}'");
                            continue;
                        }

                        if (ability.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(ability.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            ability.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                        }

                        newAbilities.Add(ability);
                    }

                    if (diagnostics.Count > 0)
                    {
                        var memoryStream = new MemoryStream();
                        using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                        writer.Write(new MapAbilityObjectData(mapAbilityObjectData.FormatVersion)
                        {
                            BaseAbilities = baseAbilities,
                            NewAbilities = newAbilities,
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