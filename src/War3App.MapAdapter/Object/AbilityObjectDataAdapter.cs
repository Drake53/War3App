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
    public sealed class AbilityObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Ability)";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), MapAbilityObjectData.FileName.GetFileExtension(), StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var mapAbilityObjectData = MapAbilityObjectData.Parse(stream);

                try
                {
                    var knownIds = AbilityObjectDataProvider.GetRawcodes(targetPatch).ToHashSet();
                    var knownProperties = AbilityObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet();

                    var diagnostics = new List<string>();
                    var abilities = new List<ObjectModification>();
                    foreach (var ability in mapAbilityObjectData.GetData())
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

                        if (ability.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(ability.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            abilities.Add(new ObjectModification(ability.OldId, ability.NewId, ability.Where(property => knownProperties.Contains(property.Id)).ToArray()));
                        }
                        else
                        {
                            abilities.Add(ability);
                        }
                    }

                    if (diagnostics.Count > 0)
                    {
                        var memoryStream = new MemoryStream();
                        MapAbilityObjectData.Serialize(new MapAbilityObjectData(abilities.ToArray()), memoryStream, true);

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
            catch
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.ParseError,
                };
            }
        }
    }
}