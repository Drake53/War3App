using System.Collections.Generic;
using System.IO;

namespace War3App.MapAdapter.Extensions
{
    public static class StringExtensions
    {
        public static string GetFileExtension(this string s)
        {
            return new FileInfo(s).Extension;
        }

        internal static string[] GetFileNotFoundDiagnostics(this string path)
        {
            var result = new List<string>();
            result.Add("Required file not found: " + path);

            var directoryInfo = new DirectoryInfo(path).Parent;
            while (directoryInfo is not null)
            {
                if (directoryInfo is null)
                {
                    break;
                }

                if (!directoryInfo.Exists)
                {
                    result.Add("Directory not found: " + directoryInfo.FullName);
                }

                directoryInfo = directoryInfo.Parent;
            }

            return result.ToArray();
        }
    }
}