using System.Collections.Generic;

namespace War3App.MapDowngrader
{
    public static class ReforgedIdentifiersProvider
    {
        public static IEnumerable<string> GetIdentifiers()
        {
            // 1.32 beta-4 (common.j):
            yield return "MathRound";
            yield return "SetEnemyStartLocPrioCount";
            yield return "SetEnemyStartLocPrio";
            yield return "ParseTags";
            yield return "TriggerRegisterCommandEvent";
            yield return "TriggerRegisterUpgradeCommandEvent";
            yield return "GetPlayerHandicapReviveTime";
            yield return "GetPlayerHandicapDamage";
            yield return "SetPlayerHandicapReviveTime";
            yield return "SetPlayerHandicapDamage";
            yield return "SetMaxCheckpointSaves";
            yield return "SaveGameCheckpoint";
            yield return "SetPortraitLight";
            yield return "CreateMinimapIconOnUnit";
            yield return "CreateMinimapIconAtLoc";
            yield return "CreateMinimapIcon";
            yield return "SkinManagerGetLocalPath";
            yield return "DestroyMinimapIcon";
            yield return "SetMinimapIconVisible";
            yield return "SetMinimapIconOrphanDestroy";
            yield return "BlzCameraSetupSetLabel";
            yield return "BlzCameraSetupGetLabel";
            yield return "CameraSetFocalDistance";
            yield return "CameraSetDepthOfFieldScale";
            yield return "SetSoundFacialAnimationLabel";
            yield return "SetSoundFacialAnimationGroupLabel";
            yield return "SetSoundFacialAnimationSetFilepath";
            yield return "SetDialogueSpeakerNameKey";
            yield return "GetDialogueSpeakerNameKey";
            yield return "SetDialogueTextKey";
            yield return "GetDialogueTextKey";
            yield return "BlzHideCinematicPanels";
            yield return "BlzShowTerrain";
            yield return "BlzShowSkyBox";
            yield return "BlzStartRecording";
            yield return "BlzEndRecording";
            yield return "BlzShowUnitTeamGlow";
            yield return "CreateCommandButtonEffect";
            yield return "CreateUpgradeCommandButtonEffect";
            yield return "CreateLearnCommandButtonEffect";
            yield return "DestroyCommandButtonEffect";
            yield return "BlzGetUnitSkin";
            yield return "BlzGetItemSkin";
            yield return "BlzSetUnitSkin";
            yield return "BlzSetItemSkin";
            yield return "BlzCreateItemWithSkin";
            yield return "BlzCreateUnitWithSkin";
            yield return "BlzCreateDestructableWithSkin";
            yield return "BlzCreateDestructableZWithSkin";
            yield return "BlzCreateDeadDestructableWithSkin";
            yield return "BlzCreateDeadDestructableZWithSkin";
            yield return "BlzGetPlayerTownHallCount";

            // 1.32 beta-4 (Blizzard.j):
            yield return "bj_HANDICAP_NORMAL";
            yield return "bj_HANDICAPDAMAGE_EASY";
            yield return "bj_HANDICAPDAMAGE_NORMAL";
            yield return "bj_HANDICAPREVIVE_NOTHARD";
            yield return "bj_MAX_CHECKPOINTS";
            yield return "bj_MISSION_INDEX_T02";
            yield return "bj_MISSION_INDEX_T03";
            yield return "bj_MISSION_INDEX_T04";
            yield return "bj_CAMPPINGSTYLE_PRIMARY";
            yield return "bj_CAMPPINGSTYLE_PRIMARY_GREEN";
            yield return "bj_CAMPPINGSTYLE_PRIMARY_RED";
            yield return "bj_CAMPPINGSTYLE_BONUS";
            yield return "bj_CAMPPINGSTYLE_TURNIN";
            yield return "bj_CAMPPINGSTYLE_BOSS";
            yield return "bj_CAMPPINGSTYLE_CONTROL_ALLY";
            yield return "bj_CAMPPINGSTYLE_CONTROL_NEUTRAL";
            yield return "bj_CAMPPINGSTYLE_CONTROL_ENEMY";
            yield return "bj_lastCreatedMinimapIcon";
            yield return "bj_lastCreatedCommandButtonEffect";
            yield return "CameraSetupApplyForPlayerSmooth";
            yield return "TriggerRegisterBuildCommandEventBJ";
            yield return "TriggerRegisterTrainCommandEventBJ";
            yield return "TriggerRegisterUpgradeCommandEventBJ";
            yield return "TriggerRegisterCommonCommandEventBJ";
            yield return "GetLastCreatedMinimapIcon";
            yield return "CreateMinimapIconOnUnitBJ";
            yield return "CreateMinimapIconAtLocBJ";
            yield return "CreateMinimapIconBJ";
            yield return "CampaignMinimapIconUnitBJ";
            yield return "CampaignMinimapIconLocBJ";
            yield return "CreateCommandButtonEffectBJ";
            yield return "CreateTrainCommandButtonEffectBJ";
            yield return "CreateUpgradeCommandButtonEffectBJ";
            yield return "CreateCommonCommandButtonEffectBJ";
            yield return "CreateLearnCommandButtonEffectBJ";
            yield return "CreateBuildCommandButtonEffectBJ";
            yield return "GetLastCreatedCommandButtonEffectBJ";
            yield return "SetPlayerHandicapDamageBJ";
            yield return "GetPlayerHandicapDamageBJ";
            yield return "SetPlayerHandicapReviveTimeBJ";
            yield return "GetPlayerHandicapReviveTimeBJ";
            yield return "PlayDialogueFromSpeakerEx";
            yield return "PlayDialogueFromSpeakerTypeEx";
            yield return "SaveGameCheckPointBJ";

            // 1.32.0 (common.j):
            yield return "ORIGIN_FRAME_SIMPLE_UI_PARENT";
            yield return "ORIGIN_FRAME_PORTRAIT_HP_TEXT";
            yield return "ORIGIN_FRAME_PORTRAIT_MANA_TEXT";
            yield return "ORIGIN_FRAME_UNIT_PANEL_BUFF_BAR";
            yield return "ORIGIN_FRAME_UNIT_PANEL_BUFF_BAR_LABEL";
            yield return "SetCinematicAudio";
            yield return "BlzStartUnitAbilityCooldown";
            yield return "BlzGetEventIsAttack";
            yield return "BlzSetUnitFacingEx";

            // TODO: check if this was added in v1.32.1 or v1.32.2, and add both versions to GamePatch enum.
            // 1.32.2
            yield return "SetThematicMusicVolume";
            yield return "SetThematicMusicVolumeBJ";
        }
    }
}