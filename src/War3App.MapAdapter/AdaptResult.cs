using System;
using System.IO;

using War3App.MapAdapter.Diagnostics;

namespace War3App.MapAdapter
{
    public sealed class AdaptResult : IDisposable
    {
        private AdaptResult(MapFileStatus status)
        {
            Status = status;
            AdaptedFileStream = null;
        }

        private AdaptResult(Stream stream, MapFileStatus status)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Status = status;
            AdaptedFileStream = stream;
        }

        public MapFileStatus Status { get; set; }

        public Stream? AdaptedFileStream { get; set; }

        public Diagnostic[]? Diagnostics { get; set; }

        public static implicit operator AdaptResult(MapFileStatus status) => new(status);

        public static AdaptResult Create(Stream stream, MapFileStatus status)
        {
            if (status != MapFileStatus.Compatible &&
                status != MapFileStatus.Inconclusive &&
                status != MapFileStatus.Incompatible)
            {
                throw new ArgumentException($"Invalid result status: '{status}'.", nameof(status));
            }

            return new AdaptResult(stream, status);
        }

        public static AdaptResult ModifiedByUser(Stream stream) => new(stream, MapFileStatus.Pending);

        public void Dispose()
        {
            AdaptedFileStream?.Dispose();
        }
    }
}