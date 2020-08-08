using System.IO;

namespace War3App.MapAdapter.Extensions
{
    public static class StringExtensions
    {
        public static string GetFileExtension(this string s)
        {
            return new FileInfo(s).Extension;
        }
    }
}