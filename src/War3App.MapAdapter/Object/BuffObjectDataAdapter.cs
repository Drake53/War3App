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
    public sealed class BuffObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Buff)";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var mapBuffObjectData = reader.ReadMapBuffObjectData();

                try
                {
                    var knownIds = BuffObjectDataProvider.GetRawcodes(targetPatch).ToHashSet();
                    var knownProperties = BuffObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet();

                    var diagnostics = new List<string>();

                    var baseBuffs = new List<SimpleObjectModification>();
                    foreach (var buff in mapBuffObjectData.BaseBuffs)
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
                    foreach (var buff in mapBuffObjectData.NewBuffs)
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

                    if (diagnostics.Count > 0)
                    {
                        var memoryStream = new MemoryStream();
                        using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                        writer.Write(new MapBuffObjectData(mapBuffObjectData.FormatVersion)
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