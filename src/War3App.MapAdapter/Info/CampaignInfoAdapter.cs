using System;
using System.IO;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Info;

namespace War3App.MapAdapter.Info
{
    public sealed class CampaignInfoAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Campaign Info";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), CampaignInfo.FileName.GetFileExtension(), StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var campaignInfo = CampaignInfo.Parse(stream);

                var targetPatchEditorVersion = targetPatch.GetEditorVersion();
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
                    campaignInfo.SerializeTo(newCampaignInfoFileStream, true);

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