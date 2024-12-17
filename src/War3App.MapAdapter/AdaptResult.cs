using System;
using System.IO;

using War3App.MapAdapter.Diagnostics;

namespace War3App.MapAdapter
{
    public sealed class AdaptResult : IDisposable
    {
        public AdaptResult(MapFileStatus status)
        {
            Status = status;
            AdaptedFileStream = null;
        }

        public AdaptResult(Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Status = MapFileStatus.Adapted;
            AdaptedFileStream = stream;
        }

        public MapFileStatus Status { get; set; }

        public Stream? AdaptedFileStream { get; set; }

        public Diagnostic[]? Diagnostics { get; set; }

        public static implicit operator AdaptResult(MapFileStatus status) => new(status);

        public static implicit operator AdaptResult(Stream stream) => new(stream);

        public void Dispose()
        {
            AdaptedFileStream?.Dispose();
        }
    }
}