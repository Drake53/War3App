using System;
using System.Collections.Generic;
using System.Linq;

using War3App.MapAdapter.Extensions;

namespace War3App.MapAdapter.WinForms
{
    internal static class PatchAgnosticFileProvider
    {
        private static readonly HashSet<string> _patchAgnosticFiles = InitPatchAgnosticFiles().ToHashSet(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> _patchAgnosticFileExtensions = InitPatchAgnosticFileExtensions().ToHashSet(StringComparer.OrdinalIgnoreCase);

        public static bool IsFilePatchAgnostic(string fileName)
        {
            return _patchAgnosticFiles.Contains(fileName)
                || _patchAgnosticFileExtensions.Contains(fileName.GetFileExtension());
        }

        private static IEnumerable<string> InitPatchAgnosticFiles()
        {
            yield return "(listfile)";
            yield return "(attributes)";
        }

        private static IEnumerable<string> InitPatchAgnosticFileExtensions()
        {
            yield return ".mp3";
            yield return ".wav";

            yield return ".blp";
            yield return ".tga";
        }
    }
}