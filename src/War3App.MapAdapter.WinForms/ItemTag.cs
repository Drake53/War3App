﻿using System;
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

        public ItemTag(MpqArchive archive, MpqEntry mpqEntry, string archiveName = null)
        {
            MpqEntry = mpqEntry;
            FileName = mpqEntry.FileName;
            ArchiveName = archiveName ?? string.Empty;
            OriginalFileStream = archive.OpenFile(mpqEntry);

            if (!OriginalFileStream.CanRead)
            {
                OriginalFileStream.Dispose();
                Status = MapFileStatus.Locked;
            }
            else
            {
                Adapter = AdapterFactory.GetAdapter(OriginalFileStream, FileName);
                Status = Adapter is null ? MapFileStatus.Unknown : MapFileStatus.Pending;
            }
        }

        public ItemTag(MpqArchive archive, MpqEntry mpqEntry, ListViewItem[] children, GamePatch? originPatch)
        {
            MpqEntry = mpqEntry;
            FileName = mpqEntry.FileName;
            ArchiveName = string.Empty;
            Adapter = null;
            OriginalFileStream = archive.OpenFile(mpqEntry);

            Children = children.Select(child => child.GetTag()).ToArray();
            foreach (var child in Children)
            {
                child.Parent = this;
            }

            OriginPatch = originPatch;
        }

        public MpqEntry MpqEntry { get; private set; }

        public string? FileName { get; private set; }

        public string ArchiveName { get; private set; }

        public IMapFileAdapter? Adapter { get; private set; }

        public MpqStream OriginalFileStream { get; private set; }

        public ItemTag Parent { get; private set; }

        public ItemTag[] Children { get; private set; }

        public GamePatch? OriginPatch { get; private set; }

        public ListViewItem ListViewItem { get; set; }

        public AdaptResult? AdaptResult { get; set; }

        public MapFileStatus Status
        {
            get => AdaptResult?.Status ?? _status;
            set => _status = value;
        }

        public Stream CurrentStream => AdaptResult?.AdaptedFileStream ?? OriginalFileStream;

        public bool IsModified => AdaptResult?.AdaptedFileStream != null;

        public void UpdateAdaptResult(AdaptResult adaptResult)
        {
            if (adaptResult.AdaptedFileStream == null && AdaptResult?.AdaptedFileStream != null)
            {
                adaptResult.AdaptedFileStream = AdaptResult.AdaptedFileStream;

                if (adaptResult.Status == MapFileStatus.Compatible && Status == MapFileStatus.Modified)
                {
                    adaptResult.Status = MapFileStatus.Adapted;
                }
            }

            ListViewItem.Update(adaptResult);
        }

        public bool TryGetModifiedMpqFile(out MpqFile mpqFile)
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
            OriginalFileStream.Dispose();
            AdaptResult?.Dispose();
        }
    }
}