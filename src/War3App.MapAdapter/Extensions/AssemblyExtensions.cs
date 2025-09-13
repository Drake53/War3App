using System;
using System.Diagnostics;
using System.Reflection;

namespace War3App.MapAdapter.Extensions
{
    public static class AssemblyExtensions
    {
        public static string GetVersionString(this Assembly assembly)
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            if (Version.TryParse(versionInfo.FileVersion, out var version))
            {
                return version.ToString(3);
            }

            return "1.0.0";
        }

        public static string GetFullVersionString(this Assembly assembly)
        {
            return FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion ?? string.Empty;
        }
    }
}