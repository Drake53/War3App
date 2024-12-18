using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using War3App.MapAdapter.WinForms.Extensions;

using War3Net.Build.Common;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.WinForms
{
    public sealed class ItemTag : IDisposable
    {
        private MapFileStatus _status;

        public ItemTag(MpqArchive archive, MpqEntry mpqEntry, int originalIndex, string? archiveName = null)
        {
            MpqArchive = archive;
            MpqEntry = mpqEntry;
            FileName = mpqEntry.FileName;
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
                Adapter = AdapterFactory.GetAdapter(OriginalFileStream, FileName);
                Status = Adapter is null ? MapFileStatus.Unknown : MapFileStatus.Pending;
            }
        }

        public ItemTag(MpqArchive archive, MpqEntry mpqEntry, int originalIndex, ListViewItem[] children, GamePatch? originPatch)
        {
            MpqArchive = archive;
            MpqEntry = mpqEntry;
            FileName = mpqEntry.FileName;
            ArchiveName = string.Empty;
            Adapter = null;
            OriginalFileStream = archive.OpenFile(mpqEntry);
            OriginalIndex = originalIndex;

            Children = children.Select(child => child.GetTag()).ToArray();
            foreach (var child in Children)
            {
                child.Parent = this;
            }

            OriginPatch = originPatch;
        }

        public MpqArchive MpqArchive { get; }

        public MpqEntry MpqEntry { get; }

        public string? FileName { get; }

        public string ArchiveName { get; }

        public IMapFileAdapter? Adapter { get; }

        public MpqStream? OriginalFileStream { get; }

        public int OriginalIndex { get; }

        public ItemTag? Parent { get; private set; }

        public ItemTag[]? Children { get; }

        public GamePatch? OriginPatch { get; }

        public ListViewItem ListViewItem { get; set; }

        public AdaptResult? AdaptResult { get; set; }

        public MapFileStatus Status
        {
            get => AdaptResult?.Status ?? _status;
            set => _status = value;
        }

        public Stream? CurrentStream => AdaptResult?.AdaptedFileStream ?? OriginalFileStream;

        [MemberNotNullWhen(true, nameof(AdaptResult))]
        public bool IsModified => AdaptResult?.AdaptedFileStream is not null;

        public void UpdateAdaptResult(AdaptResult adaptResult)
        {
            if (AdaptResult?.AdaptedFileStream is not null)
            {
                if (adaptResult.AdaptedFileStream is null)
                {
                    adaptResult.AdaptedFileStream = AdaptResult.AdaptedFileStream;

                    if (adaptResult.Status == MapFileStatus.Compatible && Status == MapFileStatus.Pending)
                    {
                        adaptResult.Status = MapFileStatus.Adapted;
                    }
                }
                else
                {
                    AdaptResult.Dispose();
                }
            }

            ListViewItem.Update(adaptResult);
        }

        public bool TryGetModifiedMpqFile([NotNullWhen(true)] out MpqFile? mpqFile)
        {
            if (IsModified)
            {
                AdaptResult.AdaptedFileStream.Position = 0;
                mpqFile = MpqFile.New(AdaptResult.AdaptedFileStream, FileName, true);
                mpqFile.TargetFlags = MpqEntry.Flags;

                return true;
            }

            mpqFile = null;
            return false;
        }

        public bool TryGetHashedFileName(out ulong hashedFileName)
        {
            if (FileName is null)
            {
                hashedFileName = default;
                return false;
            }

            hashedFileName = MpqHash.GetHashedFileName(FileName);
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