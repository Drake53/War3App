using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Script
{
    public sealed class MapScriptRegexAdapter : IMapFileAdapter
    {
        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch)
        {
            try
            {
                string scriptText;
                using (var reader = new StreamReader(stream, leaveOpen: true))
                {
                    scriptText = reader.ReadToEnd();
                }

                try
                {
                    // todo: add logging to adaptResult
                    var diagnostics = new List<string>();

                    // Find incompatible identifiers
                    var incompatibleIdentifiers = new HashSet<string>();
                    if (targetPatch < GamePatch.v1_32_5)
                    {
                        incompatibleIdentifiers.UnionWith(CommonIdentifiersProvider.GetIdentifiers(targetPatch, GamePatch.v1_32_5 /*originPatch.Value*/));
                        incompatibleIdentifiers.UnionWith(BlizzardIdentifiersProvider.GetIdentifiers(targetPatch, GamePatch.v1_32_5 /*originPatch.Value*/));
                    }

                    foreach (var incompatibleIdentifier in incompatibleIdentifiers)
                    {
                        var matches = new Regex($"\\b{incompatibleIdentifier}\\b").Matches(scriptText);
                        var usageCount = matches.Count;
                        if (usageCount > 0)
                        {
                            diagnostics.Add($"Found incompatible identifier: '{incompatibleIdentifier}' ({usageCount}x)");
                        }
                    }

                    // Find incompatible audio formats
                    var incompatibleAudioFormats = new HashSet<string>();
                    if (targetPatch < GamePatch.v1_32_0)
                    {
                        incompatibleAudioFormats.Add("flac");
                    }
                    if (targetPatch < GamePatch.v1_30_0 || targetPatch > GamePatch.v1_30_4)
                    {
                        incompatibleAudioFormats.Add("ogg");
                    }

                    foreach (var incompatibleAudioFormat in incompatibleAudioFormats)
                    {
                        var matches = new Regex($"\"(\\w|/)+.{incompatibleAudioFormat}\"").Matches(scriptText);
                        var usageCount = matches.Count;
                        if (usageCount > 0)
                        {
                            diagnostics.Add($"Found incompatible audio formats: '{incompatibleAudioFormat}' ({usageCount}x)");
                            //foreach (Match match in matches)
                            //{
                            //    if (incompatibleAudioFileUsage.TryGetValue(match.Value, out var value))
                            //    {
                            //        incompatibleAudioFileUsage[match.Value] = value + 1;
                            //    }
                            //    else
                            //    {
                            //        incompatibleAudioFileUsage.Add(match.Value, 1);
                            //    }
                            //}
                        }
                    }

                    // Find incompatible frame names
                    var incompatibleFrameNames = new HashSet<string>();
                    if (targetPatch >= GamePatch.v1_31_0)
                    {
                        incompatibleFrameNames.UnionWith(FrameNamesProvider.GetFrameNames(targetPatch, GamePatch.v1_32_5 /*originPatch.Value*/).Select(frame => frame.name));
                    }

                    foreach (var incompatibleFrameName in incompatibleFrameNames)
                    {
                        var matches = new Regex($"{nameof(War3Api.Common.BlzGetFrameByName)}( |\t)*\\(\"{incompatibleFrameName}\"( |\t)*,( |\t)*").Matches(scriptText);
                        var usageCount = matches.Count;
                        if (usageCount > 0)
                        {
                            diagnostics.Add($"Found incompatible frame names: '{incompatibleFrameName}' ({usageCount}x)");
                        }
                    }

                    if (diagnostics.Count == 0)
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.Compatible,
                        };
                    }
                    else
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.RequiresInput,
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