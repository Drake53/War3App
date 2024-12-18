using War3App.MapAdapter.Extensions;

using War3Net.Build.Info;

namespace War3App.MapAdapter.Info
{
    public static class CampaignInfoExtensions
    {
        public static bool Adapt(this CampaignInfo campaignInfo, AdaptFileContext context, out MapFileStatus status)
        {
            status = MapFileStatus.Compatible;

            var targetPatchEditorVersion = context.TargetPatch.Patch.GetEditorVersion();
            if (campaignInfo.EditorVersion == targetPatchEditorVersion)
            {
                return false;
            }

            campaignInfo.EditorVersion = targetPatchEditorVersion;
            return true;
        }
    }
}