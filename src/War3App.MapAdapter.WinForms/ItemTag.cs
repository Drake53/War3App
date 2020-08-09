using System.IO;
using System.Linq;
using System.Windows.Forms;

using War3App.MapAdapter.WinForms.Extensions;

using War3Net.IO.Mpq;

namespace War3App.MapAdapter.WinForms
{
    public sealed class ItemTag
    {
        private MapFileStatus _status;

        public ItemTag(MpqArchive archive, MpqEntry mpqEntry, string archiveName = null)
        {
            MpqEntry = mpqEntry;
            FileName = mpqEntry.Filename;
            ArchiveName = archiveName ?? string.Empty;
            OriginalFileStream = archive.OpenFile(mpqEntry);
            if (FileName != null && PatchAgnosticFileProvider.IsFilePatchAgnostic(FileName))
            {
                Status = MapFileStatus.Compatible;
            }
            else
            {
                Adapter = AdapterProvider.GetAdapter(this);
                Status = Adapter is null ? MapFileStatus.Unknown : MapFileStatus.Pending;
            }
        }

        public ItemTag(MpqArchive archive, MpqEntry mpqEntry, ListViewItem[] children)
        {
            MpqEntry = mpqEntry;
            FileName = mpqEntry.Filename;
            ArchiveName = string.Empty;
            Adapter = null;
            OriginalFileStream = archive.OpenFile(mpqEntry);

            Children = children.Select(child => child.GetTag()).ToArray();
            foreach (var child in Children)
            {
                child.Parent = this;
            }
        }

        public MpqEntry MpqEntry { get; private set; }

        public string? FileName { get; private set; }

        public string ArchiveName { get; private set; }

        public IMapFileAdapter Adapter { get; private set; }

        public Stream OriginalFileStream { get; private set; }

        public ItemTag Parent { get; private set; }

        public ItemTag[] Children { get; private set; }

        public ListViewItem ListViewItem { get; set; }

        public AdaptResult? AdaptResult { get; set; }

        public MapFileStatus Status
        {
            get => AdaptResult?.Status ?? _status;
            set => _status = value;
        }

        public Stream CurrentStream => AdaptResult?.AdaptedFileStream ?? OriginalFileStream;

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
    }
}