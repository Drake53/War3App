using System;
using System.Collections.Generic;

using War3Net.Build.Common;

namespace War3App.MapDowngrader
{
    public static class BlizzardIdentifiersProvider
    {
        public static IEnumerable<string> GetIdentifiers(GamePatch from, GamePatch to)
        {
            for (var patch = from + 1; from <= to; patch++)
            {
                foreach (var identifier in GetIdentifiers(patch))
                {
                    yield return identifier;
                }
            }
        }

        public static IEnumerable<string> GetIdentifiers(GamePatch patch)
        {
            return patch switch
            {
                // TODO: other patches
                GamePatch.v1_32_0 => GetIdentifiersPatch1_32_0(),

                _ => Array.Empty<string>(),
            };
        }

        private static IEnumerable<string> GetIdentifiersPatch1_32_0()
        {
            // 1.32 beta-4
            yield return nameof(War3Api.Blizzard.bj_HANDICAP_NORMAL);
            yield return nameof(War3Api.Blizzard.bj_HANDICAPDAMAGE_EASY);
            yield return nameof(War3Api.Blizzard.bj_HANDICAPDAMAGE_NORMAL);
            yield return nameof(War3Api.Blizzard.bj_HANDICAPREVIVE_NOTHARD);
            yield return nameof(War3Api.Blizzard.bj_MAX_CHECKPOINTS);
            yield return nameof(War3Api.Blizzard.bj_MISSION_INDEX_T02);
            yield return nameof(War3Api.Blizzard.bj_MISSION_INDEX_T03);
            yield return nameof(War3Api.Blizzard.bj_MISSION_INDEX_T04);
            yield return nameof(War3Api.Blizzard.bj_CAMPPINGSTYLE_PRIMARY);
            yield return nameof(War3Api.Blizzard.bj_CAMPPINGSTYLE_PRIMARY_GREEN);
            yield return nameof(War3Api.Blizzard.bj_CAMPPINGSTYLE_PRIMARY_RED);
            yield return nameof(War3Api.Blizzard.bj_CAMPPINGSTYLE_BONUS);
            yield return nameof(War3Api.Blizzard.bj_CAMPPINGSTYLE_TURNIN);
            yield return nameof(War3Api.Blizzard.bj_CAMPPINGSTYLE_BOSS);
            yield return nameof(War3Api.Blizzard.bj_CAMPPINGSTYLE_CONTROL_ALLY);
            yield return nameof(War3Api.Blizzard.bj_CAMPPINGSTYLE_CONTROL_NEUTRAL);
            yield return nameof(War3Api.Blizzard.bj_CAMPPINGSTYLE_CONTROL_ENEMY);
            yield return nameof(War3Api.Blizzard.bj_lastCreatedMinimapIcon);
            yield return nameof(War3Api.Blizzard.bj_lastCreatedCommandButtonEffect);
            yield return nameof(War3Api.Blizzard.CameraSetupApplyForPlayerSmooth);
            yield return nameof(War3Api.Blizzard.TriggerRegisterBuildCommandEventBJ);
            yield return nameof(War3Api.Blizzard.TriggerRegisterTrainCommandEventBJ);
            yield return nameof(War3Api.Blizzard.TriggerRegisterUpgradeCommandEventBJ);
            yield return nameof(War3Api.Blizzard.TriggerRegisterCommonCommandEventBJ);
            yield return nameof(War3Api.Blizzard.GetLastCreatedMinimapIcon);
            yield return nameof(War3Api.Blizzard.CreateMinimapIconOnUnitBJ);
            yield return nameof(War3Api.Blizzard.CreateMinimapIconAtLocBJ);
            yield return nameof(War3Api.Blizzard.CreateMinimapIconBJ);
            yield return nameof(War3Api.Blizzard.CampaignMinimapIconUnitBJ);
            yield return nameof(War3Api.Blizzard.CampaignMinimapIconLocBJ);
            yield return nameof(War3Api.Blizzard.CreateCommandButtonEffectBJ);
            yield return nameof(War3Api.Blizzard.CreateTrainCommandButtonEffectBJ);
            yield return nameof(War3Api.Blizzard.CreateUpgradeCommandButtonEffectBJ);
            yield return nameof(War3Api.Blizzard.CreateCommonCommandButtonEffectBJ);
            yield return nameof(War3Api.Blizzard.CreateLearnCommandButtonEffectBJ);
            yield return nameof(War3Api.Blizzard.CreateBuildCommandButtonEffectBJ);
            yield return nameof(War3Api.Blizzard.GetLastCreatedCommandButtonEffectBJ);
            yield return nameof(War3Api.Blizzard.SetPlayerHandicapDamageBJ);
            yield return nameof(War3Api.Blizzard.GetPlayerHandicapDamageBJ);
            yield return nameof(War3Api.Blizzard.SetPlayerHandicapReviveTimeBJ);
            yield return nameof(War3Api.Blizzard.GetPlayerHandicapReviveTimeBJ);
            yield return nameof(War3Api.Blizzard.PlayDialogueFromSpeakerEx);
            yield return nameof(War3Api.Blizzard.PlayDialogueFromSpeakerTypeEx);
            yield return nameof(War3Api.Blizzard.SaveGameCheckPointBJ);
        }

        private static IEnumerable<string> GetIdentifiersPatch1_32_1()
        {
            yield break;
        }

        private static IEnumerable<string> GetIdentifiersPatch1_32_2()
        {
            // Could have been added in 1.31.1 as well.
            yield return nameof(War3Api.Blizzard.SetThematicMusicVolumeBJ);
        }
    }
}