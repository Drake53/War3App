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
    public sealed class BuffObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Buff)";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), MapBuffObjectData.FileName.GetFileExtension(), StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var mapBuffObjectData = MapBuffObjectData.Parse(stream);

                try
                {
                    var knownIds = BuffObjectDataProvider.GetRawcodes(targetPatch).ToHashSet();
                    var knownProperties = BuffObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet();

                    var diagnostics = new List<string>();
                    var buffs = new List<ObjectModification>();
                    foreach (var buff in mapBuffObjectData.GetData())
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

                        if (buff.Any(property => !knownProperties.Contains(property.Id)))
                        {
                            diagnostics.AddRange(buff.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                            buffs.Add(new ObjectModification(buff.OldId, buff.NewId, buff.Where(property => knownProperties.Contains(property.Id)).ToArray()));
                        }
                        else
                        {
                            buffs.Add(buff);
                        }
                    }

                    if (diagnostics.Count > 0)
                    {
                        var memoryStream = new MemoryStream();
                        MapBuffObjectData.Serialize(new MapBuffObjectData(buffs.ToArray()), memoryStream, true);

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