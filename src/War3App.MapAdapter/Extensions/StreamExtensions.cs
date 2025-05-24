using System.IO;

namespace War3App.MapAdapter.Extensions
{
    public static class StreamExtensions
    {
        public static string ReadAllText(this Stream stream, bool leaveOpen = false)
        {
            using var reader = new StreamReader(stream, leaveOpen: leaveOpen);

            return reader.ReadToEnd();
        }
    }
}