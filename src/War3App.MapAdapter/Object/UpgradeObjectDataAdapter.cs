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
    public sealed class UpgradeObjectDataAdapter : IMapFileAdapter
    {
        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), MapUpgradeObjectData.FileName.GetFileExtension(), StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch)
        {
            try
            {
                var mapUpgradeObjectData = MapUpgradeObjectData.Parse(stream);

                try
                {
                    var knownIds = UpgradeObjectDataProvider.GetRawcodes(targetPatch).ToHashSet();
                    var knownProperties = UpgradeObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet();

                    var diagnostics = new List<string>();
                    var upgrades = new List<ObjectModification>();
                    foreach (var upgrade in mapUpgradeObjectData.GetData())
                    {
                        if (!knownIds.Contains(upgrade.OldId))
                        {
                            diagnostics.Add($"Unknown base upgrade: '{upgrade.OldId.ToRawcode()}'");
                            continue;
                        }

                        if (knownIds.Contains(upgrade.NewId))
                        {
                            diagnostics.Add($"Conflicting upgrade: '{upgrade.NewId.ToRawcode()}'");
                            continue;
                        }

                        if (upgrade.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(upgrade.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            upgrades.Add(new ObjectModification(upgrade.OldId, upgrade.NewId, upgrade.Where(property => knownProperties.Contains(property.Id)).ToArray()));
                        }
                        else
                        {
                            upgrades.Add(upgrade);
                        }
                    }

                    if (diagnostics.Count > 0)
                    {
                        var memoryStream = new MemoryStream();
                        MapUpgradeObjectData.Serialize(new MapUpgradeObjectData(upgrades.ToArray()), memoryStream, true);

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