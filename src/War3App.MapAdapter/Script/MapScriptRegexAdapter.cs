using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Script
{
    public sealed class MapScriptRegexAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Map Script";

        public bool IsTextFile => true;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                string scriptText;
                using (var reader = new StreamReader(stream, null, true, -1, true))
                {
                    scriptText = reader.ReadToEnd();
                }

                try
                {
                    var diagnostics = new List<string>();
                    var regexDiagnostics = new List<RegexDiagnostic>();

                    // Find incompatible types
                    var incompatibleTypes = new HashSet<string>(CommonTypesProvider.GetTypes(targetPatch, originPatch));

                    foreach (var incompatibleType in incompatibleTypes)
                    {
                        var regex = new Regex($"\\b{incompatibleType}\\b");
                        var matches = regex.Matches(scriptText);
                        var usageCount = matches.Count;
                        if (usageCount > 0)
                        {
                            diagnostics.Add($"Found incompatible type: '{incompatibleType}' ({usageCount}x)");
                            regexDiagnostics.Add(new RegexDiagnostic
                            {
                                DisplayText = $"Type: '{incompatibleType}'",
                                Matches = usageCount,
                                Regex = regex,
                            });
                        }
                    }

                    // Find incompatible identifiers
                    var incompatibleIdentifiers = new HashSet<string>();
                    incompatibleIdentifiers.UnionWith(CommonIdentifiersProvider.GetIdentifiers(targetPatch, originPatch));
                    incompatibleIdentifiers.UnionWith(BlizzardIdentifiersProvider.GetIdentifiers(targetPatch, originPatch));

                    foreach (var incompatibleIdentifier in incompatibleIdentifiers)
                    {
                        var regex = new Regex($"\\b{incompatibleIdentifier}\\b");
                        var matches = regex.Matches(scriptText);
                        var usageCount = matches.Count;
                        if (usageCount > 0)
                        {
                            diagnostics.Add($"Found incompatible identifier: '{incompatibleIdentifier}' ({usageCount}x)");
                            regexDiagnostics.Add(new RegexDiagnostic
                            {
                                DisplayText = $"Identifier: '{incompatibleIdentifier}'",
                                Matches = usageCount,
                                Regex = regex,
                            });
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
                        var regex = new Regex($"\"(\\w|/)+.{incompatibleAudioFormat}\"");
                        var matches = regex.Matches(scriptText);
                        var usageCount = matches.Count;
                        if (usageCount > 0)
                        {
                            diagnostics.Add($"Found incompatible audio formats: '{incompatibleAudioFormat}' ({usageCount}x)");
                            regexDiagnostics.Add(new RegexDiagnostic
                            {
                                DisplayText = $"Audio file format: '{incompatibleAudioFormat}'",
                                Matches = usageCount,
                                Regex = regex,
                            });
                        }
                    }

                    // Find incompatible frame names
                    var incompatibleFrameNames = new HashSet<string>();
                    if (targetPatch >= GamePatch.v1_31_0)
                    {
                        incompatibleFrameNames.UnionWith(FrameNamesProvider.GetFrameNames(targetPatch, originPatch).Select(frame => frame.name));
                    }

                    foreach (var incompatibleFrameName in incompatibleFrameNames)
                    {
                        var regex = new Regex($"{nameof(War3Api.Common.BlzGetFrameByName)}( |\t)*\\(\"{incompatibleFrameName}\"( |\t)*,( |\t)*");
                        var matches = regex.Matches(scriptText);
                        var usageCount = matches.Count;
                        if (usageCount > 0)
                        {
                            diagnostics.Add($"Found incompatible frame names: '{incompatibleFrameName}' ({usageCount}x)");
                            regexDiagnostics.Add(new RegexDiagnostic
                            {
                                DisplayText = $"Frame name: '{incompatibleFrameName}'",
                                Matches = usageCount,
                                Regex = regex,
                            });
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
                            Status = MapFileStatus.Incompatible,
                            Diagnostics = diagnostics.ToArray(),
                            RegexDiagnostics = regexDiagnostics.ToArray(),
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