using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Info;

namespace War3App.MapAdapter.Info
{
    public static class CampaignInfoExtensions
    {
        public static bool Adapt(this CampaignInfo campaignInfo, AdaptFileContext context, out MapFileStatus status)
        {
            var modified = campaignInfo.ModifyEditorVersion(context);

            if (campaignInfo.GetMinimumPatch() <= context.TargetPatch.Patch)
            {
                status = MapFileStatus.Compatible;
                return modified;
            }

            status = campaignInfo.TryDowngrade(context.TargetPatch.Patch)
                ? MapFileStatus.Compatible
                : MapFileStatus.Incompatible;

            return status == MapFileStatus.Compatible;
        }

        public static bool ModifyEditorVersion(this CampaignInfo campaignInfo, AdaptFileContext context)
        {
            var targetPatchEditorVersion = context.TargetPatch.Patch.GetEditorVersion();
            if (campaignInfo.EditorVersion == targetPatchEditorVersion)
            {
                return false;
            }

            campaignInfo.EditorVersion = targetPatchEditorVersion;
            return true;
        }

        public static bool TryDowngrade(this CampaignInfo campaignInfo, GamePatch targetPatch)
        {
            try
            {
                while (campaignInfo.GetMinimumPatch() > targetPatch)
                {
                    campaignInfo.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this CampaignInfo campaignInfo)
        {
            switch (campaignInfo.FormatVersion)
            {
                case CampaignInfoFormatVersion.v2:
                    campaignInfo.BackgroundVersion = CampaignBackgroundVersion.Default;
                    campaignInfo.FormatVersion = CampaignInfoFormatVersion.v1;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this CampaignInfo campaignInfo)
        {
            return campaignInfo.FormatVersion switch
            {
                CampaignInfoFormatVersion.v1 => GamePatch.v1_00,
                CampaignInfoFormatVersion.v2 => GamePatch.v2_0_3,
            };
        }
    }
}