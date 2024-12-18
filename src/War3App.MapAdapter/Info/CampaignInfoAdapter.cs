using System;
using System.IO;
using System.Text;
using System.Text.Json;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Info;
using War3Net.Common.Providers;

namespace War3App.MapAdapter.Info
{
    public sealed class CampaignInfoAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Campaign Info";

        public string DefaultFileName => CampaignInfo.FileName;

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            CampaignInfo campaignInfo;
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                campaignInfo = reader.ReadCampaignInfo();
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            if (!campaignInfo.Adapt(context, out var status))
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();

                using var writer = new BinaryWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, true);
                writer.Write(campaignInfo);

                return AdaptResult.Create(memoryStream, status);
            }
            catch (Exception e)
            {
                return context.ReportSerializeError(e);
            }
        }

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch, JsonSerializerOptions options)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            var campaignInfo = reader.ReadCampaignInfo();

            return JsonSerializer.Serialize(campaignInfo, options);
        }
    }
}