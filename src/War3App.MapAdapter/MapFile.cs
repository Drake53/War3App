using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter
{
    public sealed class MapFile : IDisposable
    {
        private MapFileStatus _status;

        public MapFile(MpqArchive archive, MpqEntry mpqEntry, int originalIndex, string? archiveName = null)
        {
            MpqArchive = archive;
            MpqEntry = mpqEntry;
            OriginalFileName = mpqEntry.FileName;
            ArchiveName = archiveName ?? string.Empty;
            OriginalFileStream = archive.OpenFile(mpqEntry);
            OriginalIndex = originalIndex;

            if (!OriginalFileStream.CanRead)
            {
                OriginalFileStream.Dispose();
                OriginalFileStream = null;
                Status = MapFileStatus.Locked;
            }
            else
            {
                Adapter = AdapterFactory.GetAdapter(OriginalFileStream, OriginalFileName);
                Status = Adapter is null ? MapFileStatus.Unknown : MapFileStatus.Pending;
            }
        }

        public MapFile(MpqArchive archive, MpqEntry mpqEntry, int originalIndex, MapFile[] children, GamePatch? originPatch)
        {
            MpqArchive = archive;
            MpqEntry = mpqEntry;
            OriginalFileName = mpqEntry.FileName;
            ArchiveName = string.Empty;
            Adapter = null;
            OriginalFileStream = archive.OpenFile(mpqEntry);
            OriginalIndex = originalIndex;

            Children = children;
            foreach (var child in Children)
            {
                child.Parent = this;
            }

            OriginPatch = originPatch;
        }

        public MpqArchive MpqArchive { get; }

        public MpqEntry MpqEntry { get; }

        public string? OriginalFileName { get; }

        public string ArchiveName { get; }

        public IMapFileAdapter? Adapter { get; }

        public MpqStream? OriginalFileStream { get; }

        public int OriginalIndex { get; }

        public MapFile? Parent { get; private set; }

        public MapFile[]? Children { get; }

        public GamePatch? OriginPatch { get; }

        public AdaptResult? AdaptResult { get; set; }

        public MapFileStatus Status
        {
            get => AdaptResult?.Status ?? _status;
            set => _status = value;
        }

        public string? CurrentFileName => AdaptResult?.NewFileName ?? OriginalFileName;

        public Stream? CurrentStream => AdaptResult?.AdaptedFileStream ?? OriginalFileStream;

        [MemberNotNullWhen(true, nameof(AdaptResult))]
        public bool IsModified => AdaptResult?.AdaptedFileStream is not null;

        public void UpdateAdaptResult(AdaptResult adaptResult)
        {
            if (AdaptResult is not null)
            {
                adaptResult.Merge(AdaptResult);
            }

            AdaptResult = adaptResult;
        }

        public bool TryGetModifiedMpqFile([NotNullWhen(true)] out MpqFile? mpqFile)
        {
            if (IsModified)
            {
                AdaptResult.AdaptedFileStream.Position = 0;
                mpqFile = MpqFile.New(AdaptResult.AdaptedFileStream, CurrentFileName, true);
                mpqFile.TargetFlags = MpqEntry.Flags;

                return true;
            }

            mpqFile = null;
            return false;
        }

        public bool TryGetHashedFileName(out ulong hashedFileName)
        {
            if (CurrentFileName is null)
            {
                hashedFileName = default;
                return false;
            }

            hashedFileName = MpqHash.GetHashedFileName(CurrentFileName);
            return true;
        }

        public GamePatch GetOriginPatch(GamePatch defaultPatch)
        {
            return OriginPatch ?? Parent?.OriginPatch ?? defaultPatch;
        }

        public void Dispose()
        {
            OriginalFileStream?.Dispose();
            AdaptResult?.Dispose();
        }
    }
}