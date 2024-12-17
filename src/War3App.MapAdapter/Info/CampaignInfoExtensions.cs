using War3App.MapAdapter.Extensions;

using War3Net.Build.Info;

namespace War3App.MapAdapter.Info
{
    public static class CampaignInfoExtensions
    {
        public static MapFileStatus Adapt(this CampaignInfo campaignInfo, AdaptFileContext context)
        {
            var targetPatchEditorVersion = context.TargetPatch.Patch.GetEditorVersion();
            if (campaignInfo.EditorVersion == targetPatchEditorVersion)
            {
                return MapFileStatus.Compatible;
            }

            campaignInfo.EditorVersion = targetPatchEditorVersion;

            return MapFileStatus.Adapted;
        }
    }
}