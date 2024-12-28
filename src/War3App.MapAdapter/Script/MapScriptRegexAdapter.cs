using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using War3App.MapAdapter.Diagnostics;

using War3Net.Build.Common;
using War3Net.Build.Script;

namespace War3App.MapAdapter.Script
{
    public sealed class MapScriptRegexAdapter : IMapFileAdapter
    {
        private static readonly MapScriptRegexAdapter _instance = new();

        private MapScriptRegexAdapter()
        {
        }

        public static MapScriptRegexAdapter Instance => _instance;

        public string MapFileDescription => "Map Script";

        public string DefaultFileName => LuaMapScript.FileName;

        public bool IsTextFile => true;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            string scriptText;
            try
            {
                using var reader = new StreamReader(stream, leaveOpen: true);
                scriptText = reader.ReadToEnd();
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            var isCompatible = true;

            // Find incompatible identifiers
            var incompatibleIdentifiers = new HashSet<string>();
            incompatibleIdentifiers.UnionWith(CommonIdentifiersProvider.GetIdentifiers(context.TargetPatch.Patch, context.OriginPatch));
            incompatibleIdentifiers.UnionWith(BlizzardIdentifiersProvider.GetIdentifiers(context.TargetPatch.Patch, context.OriginPatch));

            foreach (var incompatibleIdentifier in incompatibleIdentifiers)
            {
                var regex = new Regex($"\\b{incompatibleIdentifier}\\b");
                var matches = regex.Matches(scriptText);
                var usageCount = matches.Count;
                if (usageCount > 0)
                {
                    context.ReportRegexDiagnostic(DiagnosticRule.MapScript.UnsupportedIdentifier, regex, incompatibleIdentifier, usageCount);
                    isCompatible = false;
                }
            }

            // Find incompatible audio formats
            var incompatibleAudioFormats = new HashSet<string>();
            if (context.TargetPatch.Patch < GamePatch.v1_32_0)
            {
                incompatibleAudioFormats.Add("flac");
            }
            if (context.TargetPatch.Patch < GamePatch.v1_30_0 || context.TargetPatch.Patch > GamePatch.v1_30_4)
            {
                incompatibleAudioFormats.Add("ogg");
            }

            foreach (var incompatibleAudioFormat in incompatibleAudioFormats)
            {
                var regex = new Regex($"\"(\\w|/|\\\\)+.{incompatibleAudioFormat}\"");
                var matches = regex.Matches(scriptText);
                var usageCount = matches.Count;
                if (usageCount > 0)
                {
                    context.ReportRegexDiagnostic(DiagnosticRule.MapScript.UnsupportedAudioFormat, regex, incompatibleAudioFormat, usageCount);
                    isCompatible = false;
                }
            }

            // Find incompatible frame names
            var incompatibleFrameNames = new HashSet<string>();
            if (context.TargetPatch.Patch >= GamePatch.v1_31_0)
            {
                incompatibleFrameNames.UnionWith(FrameNamesProvider.GetFrameNames(context.TargetPatch.Patch, context.OriginPatch).Select(frame => frame.name));
            }

            foreach (var incompatibleFrameName in incompatibleFrameNames)
            {
                var regex = new Regex($"{nameof(War3Api.Common.BlzGetFrameByName)}( |\t)*\\(\"{incompatibleFrameName}\"( |\t)*,( |\t)*");
                var matches = regex.Matches(scriptText);
                var usageCount = matches.Count;
                if (usageCount > 0)
                {
                    context.ReportRegexDiagnostic(DiagnosticRule.MapScript.UnsupportedFrameName, regex, incompatibleFrameName, usageCount);
                    isCompatible = false;
                }
            }

            return isCompatible
                ? MapFileStatus.Compatible
                : MapFileStatus.Incompatible;
        }
    }
}