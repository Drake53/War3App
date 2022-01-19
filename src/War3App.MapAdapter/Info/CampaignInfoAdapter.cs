using System;
using System.IO;
using System.Text;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Info
{
    public sealed class CampaignInfoAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Campaign Info";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var campaignInfo = reader.ReadCampaignInfo();

                var targetPatchEditorVersion = targetPatch.Patch.GetEditorVersion();
                if (campaignInfo.EditorVersion == targetPatchEditorVersion)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    campaignInfo.EditorVersion = targetPatchEditorVersion;

                    var newCampaignInfoFileStream = new MemoryStream();
                    using var writer = new BinaryWriter(newCampaignInfoFileStream, new UTF8Encoding(false, true), true);
                    writer.Write(campaignInfo);

                    return new AdaptResult
                    {
                        Status = MapFileStatus.Adapted,
                        AdaptedFileStream = newCampaignInfoFileStream,
                    };
                }
                catch
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.AdapterError,
                    };
                }
            }
            catch (NotSupportedException)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.Unadaptable,
                };
            }
            catch (Exception e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.ParseError,
                    Diagnostics = new[] { e.Message },
                };
            }
        }
    }
}