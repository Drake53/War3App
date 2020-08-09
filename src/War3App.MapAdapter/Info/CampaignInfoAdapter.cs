using System;
using System.IO;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Info;

namespace War3App.MapAdapter.Info
{
    public sealed class CampaignInfoAdapter : IMapFileAdapter
    {
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
                return new AdaptResult
                {
                    Status = MapFileStatus.Compatible,
                };
            }
            catch (NotSupportedException)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.Unadaptable,
                };
            }
            catch
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.ParseError,
                };
            }
        }
    }
}