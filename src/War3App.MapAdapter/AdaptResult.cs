using System;
using System.IO;

namespace War3App.MapAdapter
{
    public sealed class AdaptResult : IDisposable
    {
        public MapFileStatus Status { get; set; }

        public Stream? AdaptedFileStream { get; set; }

        public string[]? Diagnostics { get; set; }

        public RegexDiagnostic[]? RegexDiagnostics { get; set; }

        public void Dispose()
        {
            AdaptedFileStream?.Dispose();
        }
    }
}