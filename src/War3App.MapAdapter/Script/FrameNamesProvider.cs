﻿using System;
using System.Collections.Generic;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Script
{
    // https://www.hiveworkshop.com/threads/default-names-for-blzgetframebyname.315846/
    public static class FrameNamesProvider
    {
        public static IEnumerable<(string name, int createContext)> GetFrameNames(GamePatch from, GamePatch to)
        {
            for (var patch = from + 1; patch <= to; patch++)
            {
                foreach (var identifier in GetFrameNames(patch))
                {
                    yield return identifier;
                }
            }
        }

        public static IEnumerable<(string, int)> GetFrameNames(GamePatch patch)
        {
            return patch switch
            {
                GamePatch.v1_32_0 => GetFrameNamesPatch1_32_0(),

                _ => Array.Empty<(string, int)>(),
            };
        }

        private static IEnumerable<(string, int)> GetFrameNamesPatch1_32_0()
        {
            yield return ("AllianceAcceptButton", 0);
            yield return ("AllianceAcceptButtonText", 0);
            yield return ("AllianceBackdrop", 0);
            yield return ("AllianceCancelButton", 0);
            yield return ("AllianceCancelButtonText", 0);
            yield return ("AllianceDialog", 0);
            yield return ("AllianceDialogScrollBar", 0);
            yield return ("AllianceSlot", 0);
            yield return ("AllianceSlot", 1);
            yield return ("AllianceSlot", 2);
            yield return ("AllianceSlot", 3);
            yield return ("AllianceSlot", 4);
            yield return ("AllianceSlot", 5);
            yield return ("AllianceSlot", 6);
            yield return ("AllianceSlot", 7);
            yield return ("AllianceSlot", 8);
            yield return ("AllianceSlot", 9);
            yield return ("AllianceSlot", 10);
            yield return ("AllianceSlot", 11);
            yield return ("AllianceSlot", 12);
            yield return ("AllianceSlot", 13);
            yield return ("AllianceSlot", 14);
            yield return ("AllianceSlot", 15);
            yield return ("AllianceSlot", 16);
            yield return ("AllianceSlot", 17);
            yield return ("AllianceSlot", 18);
            yield return ("AllianceSlot", 19);
            yield return ("AllianceSlot", 20);
            yield return ("AllianceSlot", 21);
            yield return ("AllianceSlot", 22);
            yield return ("AllianceSlot", 23);
            yield return ("AllianceTitle", 0);
            yield return ("AlliedVictoryCheckBox", 0);
            yield return ("AlliedVictoryLabel", 0);
            yield return ("AllyCheckBox", 0);
            yield return ("AllyCheckBox", 1);
            yield return ("AllyCheckBox", 2);
            yield return ("AllyCheckBox", 3);
            yield return ("AllyCheckBox", 4);
            yield return ("AllyCheckBox", 5);
            yield return ("AllyCheckBox", 6);
            yield return ("AllyCheckBox", 7);
            yield return ("AllyCheckBox", 8);
            yield return ("AllyCheckBox", 9);
            yield return ("AllyCheckBox", 10);
            yield return ("AllyCheckBox", 11);
            yield return ("AllyCheckBox", 12);
            yield return ("AllyCheckBox", 13);
            yield return ("AllyCheckBox", 14);
            yield return ("AllyCheckBox", 15);
            yield return ("AllyCheckBox", 16);
            yield return ("AllyCheckBox", 17);
            yield return ("AllyCheckBox", 18);
            yield return ("AllyCheckBox", 19);
            yield return ("AllyCheckBox", 20);
            yield return ("AllyCheckBox", 21);
            yield return ("AllyCheckBox", 22);
            yield return ("AllyCheckBox", 23);
            yield return ("AllyHeader", 0);
            yield return ("AmbientCheckBox", 0);
            yield return ("AmbientLabel", 0);
            yield return ("AnimQualityLabel", 0);
            yield return ("AnimQualityValue", 0);
            yield return ("BottomButtonPanel", 0);
            yield return ("CancelButtonText", 0);
            yield return ("CinematicBottomBorder", 0);
            yield return ("CinematicDialogueText", 0);
            yield return ("CinematicPanel", 0);
            yield return ("CinematicPanelControlFrame", 0);
            yield return ("CinematicPortrait", 0);
            yield return ("CinematicPortraitBackground", 0);
            yield return ("CinematicPortraitCover", 0);
            yield return ("CinematicScenePanel", 0);
            yield return ("CinematicSpeakerText", 0);
            yield return ("CinematicTopBorder", 0);
            yield return ("ColorBackdrop", 0);
            yield return ("ColorBackdrop", 1);
            yield return ("ColorBackdrop", 2);
            yield return ("ColorBackdrop", 3);
            yield return ("ColorBackdrop", 4);
            yield return ("ColorBackdrop", 5);
            yield return ("ColorBackdrop", 6);
            yield return ("ColorBackdrop", 7);
            yield return ("ColorBackdrop", 8);
            yield return ("ColorBackdrop", 9);
            yield return ("ColorBackdrop", 10);
            yield return ("ColorBackdrop", 11);
            yield return ("ColorBackdrop", 12);
            yield return ("ColorBackdrop", 13);
            yield return ("ColorBackdrop", 14);
            yield return ("ColorBackdrop", 15);
            yield return ("ColorBackdrop", 16);
            yield return ("ColorBackdrop", 17);
            yield return ("ColorBackdrop", 18);
            yield return ("ColorBackdrop", 19);
            yield return ("ColorBackdrop", 20);
            yield return ("ColorBackdrop", 21);
            yield return ("ColorBackdrop", 22);
            yield return ("ColorBackdrop", 23);
            yield return ("ColorBorder", 0);
            yield return ("ColorBorder", 1);
            yield return ("ColorBorder", 2);
            yield return ("ColorBorder", 3);
            yield return ("ColorBorder", 4);
            yield return ("ColorBorder", 5);
            yield return ("ColorBorder", 6);
            yield return ("ColorBorder", 7);
            yield return ("ColorBorder", 8);
            yield return ("ColorBorder", 9);
            yield return ("ColorBorder", 10);
            yield return ("ColorBorder", 11);
            yield return ("ColorBorder", 12);
            yield return ("ColorBorder", 13);
            yield return ("ColorBorder", 14);
            yield return ("ColorBorder", 15);
            yield return ("ColorBorder", 16);
            yield return ("ColorBorder", 17);
            yield return ("ColorBorder", 18);
            yield return ("ColorBorder", 19);
            yield return ("ColorBorder", 20);
            yield return ("ColorBorder", 21);
            yield return ("ColorBorder", 22);
            yield return ("ColorBorder", 23);
            yield return ("CommandBarFrame", 0);
            yield return ("CommandButton_0", 0);
            yield return ("CommandButton_1", 0);
            yield return ("CommandButton_10", 0);
            yield return ("CommandButton_11", 0);
            yield return ("CommandButton_2", 0);
            yield return ("CommandButton_3", 0);
            yield return ("CommandButton_4", 0);
            yield return ("CommandButton_5", 0);
            yield return ("CommandButton_6", 0);
            yield return ("CommandButton_7", 0);
            yield return ("CommandButton_8", 0);
            yield return ("CommandButton_9", 0);
            yield return ("ConfirmQuitCancelButton", 0);
            yield return ("ConfirmQuitCancelButtonText", 0);
            yield return ("ConfirmQuitMessageText", 0);
            yield return ("ConfirmQuitPanel", 0);
            yield return ("ConfirmQuitQuitButton", 0);
            yield return ("ConfirmQuitQuitButtonText", 0);
            yield return ("ConfirmQuitTitleText", 0);
            yield return ("ConsoleUI", 0);
            yield return ("ConsoleUIBackdrop", 0);
            yield return ("CustomKeysLabel", 0);
            yield return ("CustomKeysValue", 0);
            yield return ("DecoratedMapListBox", 0);
            yield return ("DeleteCancelButton", 0);
            yield return ("DeleteCancelButtonText", 0);
            yield return ("DeleteDeleteButton", 0);
            yield return ("DeleteDeleteButtonText", 0);
            yield return ("DeleteMessageText", 0);
            yield return ("DeleteOnly", 0);
            yield return ("DeleteTitleText", 0);
            yield return ("DifficultyLabel", 0);
            yield return ("DifficultyValue", 0);
            yield return ("EndGameButton", 0);
            yield return ("EndGameButtonText", 0);
            yield return ("EndGamePanel", 0);
            yield return ("EndGameTitleText", 0);
            yield return ("EnviroCheckBox", 0);
            yield return ("EnviroLabel", 0);
            yield return ("EscMenuBackdrop", 0);
            yield return ("EscMenuDeleteContainer", 0);
            yield return ("EscMenuMainPanel", 0);
            yield return ("EscMenuOptionsPanel", 0);
            yield return ("EscMenuOverwriteContainer", 0);
            yield return ("EscMenuSaveGamePanel", 0);
            yield return ("EscMenuSaveLoadContainer", 0);
            yield return ("EscOptionsLightsMenu", 0);
            yield return ("EscOptionsLightsPopupMenuArrow", 0);
            yield return ("EscOptionsLightsPopupMenuBackdrop", 0);
            yield return ("EscOptionsLightsPopupMenuDisabledBackdrop", 0);
            yield return ("EscOptionsLightsPopupMenuMenu", 0);
            yield return ("EscOptionsLightsPopupMenuTitle", 0);
            yield return ("EscOptionsOcclusionMenu", 0);
            yield return ("EscOptionsOcclusionPopupMenuArrow", 0);
            yield return ("EscOptionsOcclusionPopupMenuBackdrop", 0);
            yield return ("EscOptionsOcclusionPopupMenuDisabledBackdrop", 0);
            yield return ("EscOptionsOcclusionPopupMenuMenu", 0);
            yield return ("EscOptionsOcclusionPopupMenuTitle", 0);
            yield return ("EscOptionsParticlesMenu", 0);
            yield return ("EscOptionsParticlesPopupMenuArrow", 0);
            yield return ("EscOptionsParticlesPopupMenuBackdrop", 0);
            yield return ("EscOptionsParticlesPopupMenuDisabledBackdrop", 0);
            yield return ("EscOptionsParticlesPopupMenuMenu", 0);
            yield return ("EscOptionsParticlesPopupMenuTitle", 0);
            yield return ("EscOptionsResolutionMenu", 0);
            yield return ("EscOptionsResolutionPopupMenuArrow", 0);
            yield return ("EscOptionsResolutionPopupMenuBackdrop", 0);
            yield return ("EscOptionsResolutionPopupMenuDisabledBackdrop", 0);
            yield return ("EscOptionsResolutionPopupMenuMenu", 0);
            yield return ("EscOptionsResolutionPopupMenuTitle", 0);
            yield return ("EscOptionsShadowsMenu", 0);
            yield return ("EscOptionsShadowsPopupMenuArrow", 0);
            yield return ("EscOptionsShadowsPopupMenuBackdrop", 0);
            yield return ("EscOptionsShadowsPopupMenuDisabledBackdrop", 0);
            yield return ("EscOptionsShadowsPopupMenuMenu", 0);
            yield return ("EscOptionsShadowsPopupMenuTitle", 0);
            yield return ("EscOptionsWindowModeMenu", 0);
            yield return ("EscOptionsWindowModePopupMenuArrow", 0);
            yield return ("EscOptionsWindowModePopupMenuBackdrop", 0);
            yield return ("EscOptionsWindowModePopupMenuDisabledBackdrop", 0);
            yield return ("EscOptionsWindowModePopupMenuMenu", 0);
            yield return ("EscOptionsWindowModePopupMenuTitle", 0);
            yield return ("ExitButton", 0);
            yield return ("ExitButtonText", 0);
            yield return ("ExtraHighLatencyLabel", 0);
            yield return ("ExtraHighLatencyRadio", 0);
            yield return ("FileListFrame", 0);
            yield return ("FormationButton", 0);
            yield return ("FormationToggleCheckBox", 0);
            yield return ("FormationToggleLabel", 0);
            yield return ("GameplayButton", 0);
            yield return ("GameplayButtonText", 0);
            yield return ("GameplayPanel", 0);
            yield return ("GameplayTitleText", 0);
            yield return ("GameSpeedLabel", 0);
            yield return ("GameSpeedSlider", 0);
            yield return ("GameSpeedValue", 0);
            yield return ("GammaBrightLabel", 0);
            yield return ("GammaDarkLabel", 0);
            yield return ("GammaLabel", 0);
            yield return ("GammaSlider", 0);
            yield return ("GoldBackdrop", 0);
            yield return ("GoldBackdrop", 1);
            yield return ("GoldBackdrop", 2);
            yield return ("GoldBackdrop", 3);
            yield return ("GoldBackdrop", 4);
            yield return ("GoldBackdrop", 5);
            yield return ("GoldBackdrop", 6);
            yield return ("GoldBackdrop", 7);
            yield return ("GoldBackdrop", 8);
            yield return ("GoldBackdrop", 9);
            yield return ("GoldBackdrop", 10);
            yield return ("GoldBackdrop", 11);
            yield return ("GoldBackdrop", 12);
            yield return ("GoldBackdrop", 13);
            yield return ("GoldBackdrop", 14);
            yield return ("GoldBackdrop", 15);
            yield return ("GoldBackdrop", 16);
            yield return ("GoldBackdrop", 17);
            yield return ("GoldBackdrop", 18);
            yield return ("GoldBackdrop", 19);
            yield return ("GoldBackdrop", 20);
            yield return ("GoldBackdrop", 21);
            yield return ("GoldBackdrop", 22);
            yield return ("GoldBackdrop", 23);
            yield return ("GoldHeader", 0);
            yield return ("GoldText", 0);
            yield return ("GoldText", 1);
            yield return ("GoldText", 2);
            yield return ("GoldText", 3);
            yield return ("GoldText", 4);
            yield return ("GoldText", 5);
            yield return ("GoldText", 6);
            yield return ("GoldText", 7);
            yield return ("GoldText", 8);
            yield return ("GoldText", 9);
            yield return ("GoldText", 10);
            yield return ("GoldText", 11);
            yield return ("GoldText", 12);
            yield return ("GoldText", 13);
            yield return ("GoldText", 14);
            yield return ("GoldText", 15);
            yield return ("GoldText", 16);
            yield return ("GoldText", 17);
            yield return ("GoldText", 18);
            yield return ("GoldText", 19);
            yield return ("GoldText", 20);
            yield return ("GoldText", 21);
            yield return ("GoldText", 22);
            yield return ("GoldText", 23);
            yield return ("HDCinematicBackground", 0);
            yield return ("HDCinematicPortraitCover", 0);
            yield return ("HealthBarsCheckBox", 0);
            yield return ("HealthBarsLabel", 0);
            yield return ("HelpButton", 0);
            yield return ("HelpButtonText", 0);
            yield return ("HelpOKButton", 0);
            yield return ("HelpOKButtonText", 0);
            yield return ("HelpPanel", 0);
            yield return ("HelpTextArea", 0);
            yield return ("HelpTitleText", 0);
            yield return ("HighLatencyLabel", 0);
            yield return ("HighLatencyRadio", 0);
            yield return ("InfoPanelIconAllyFoodIcon", 7);
            yield return ("InfoPanelIconAllyFoodValue", 7);
            yield return ("InfoPanelIconAllyGoldIcon", 7);
            yield return ("InfoPanelIconAllyGoldValue", 7);
            yield return ("InfoPanelIconAllyTitle", 7);
            yield return ("InfoPanelIconAllyUpkeep", 7);
            yield return ("InfoPanelIconAllyWoodIcon", 7);
            yield return ("InfoPanelIconAllyWoodValue", 7);
            yield return ("InfoPanelIconBackdrop", 0);
            yield return ("InfoPanelIconBackdrop", 1);
            yield return ("InfoPanelIconBackdrop", 2);
            yield return ("InfoPanelIconBackdrop", 3);
            yield return ("InfoPanelIconBackdrop", 4);
            yield return ("InfoPanelIconBackdrop", 5);
            yield return ("InfoPanelIconHeroAgilityLabel", 6);
            yield return ("InfoPanelIconHeroAgilityValue", 6);
            yield return ("InfoPanelIconHeroIcon", 6);
            yield return ("InfoPanelIconHeroIntellectLabel", 6);
            yield return ("InfoPanelIconHeroIntellectValue", 6);
            yield return ("InfoPanelIconHeroStrengthLabel", 6);
            yield return ("InfoPanelIconHeroStrengthValue", 6);
            yield return ("InfoPanelIconLabel", 0);
            yield return ("InfoPanelIconLabel", 1);
            yield return ("InfoPanelIconLabel", 2);
            yield return ("InfoPanelIconLabel", 3);
            yield return ("InfoPanelIconLabel", 4);
            yield return ("InfoPanelIconLabel", 5);
            yield return ("InfoPanelIconLevel", 0);
            yield return ("InfoPanelIconLevel", 1);
            yield return ("InfoPanelIconLevel", 2);
            yield return ("InfoPanelIconLevel", 3);
            yield return ("InfoPanelIconLevel", 4);
            yield return ("InfoPanelIconLevel", 5);
            yield return ("InfoPanelIconValue", 0);
            yield return ("InfoPanelIconValue", 1);
            yield return ("InfoPanelIconValue", 2);
            yield return ("InfoPanelIconValue", 3);
            yield return ("InfoPanelIconValue", 4);
            yield return ("InfoPanelIconValue", 5);
            yield return ("InsideConfirmQuitPanel", 0);
            yield return ("InsideEndGamePanel", 0);
            yield return ("InsideHelpPanel", 0);
            yield return ("InsideMainPanel", 0);
            yield return ("InsideTipsPanel", 0);
            yield return ("InventoryButton_0", 0);
            yield return ("InventoryButton_1", 0);
            yield return ("InventoryButton_2", 0);
            yield return ("InventoryButton_3", 0);
            yield return ("InventoryButton_4", 0);
            yield return ("InventoryButton_5", 0);
            yield return ("InventoryCoverTexture", 0);
            yield return ("InventoryText", 0);
            yield return ("KeyScrollFastLabel", 0);
            yield return ("KeyScrollLabel", 0);
            yield return ("KeyScrollSlider", 0);
            yield return ("KeyScrollSlowLabel", 0);
            yield return ("LatencyInfo1", 0);
            yield return ("LatencyInfo2", 0);
            yield return ("LightsLabel", 0);
            yield return ("LoadGameButton", 0);
            yield return ("LoadGameButtonText", 0);
            yield return ("LoadGameCancelButton", 0);
            yield return ("LoadGameCancelButtonText", 0);
            yield return ("LoadGameLoadButton", 0);
            yield return ("LoadGameLoadButtonText", 0);
            yield return ("LoadGameTitleText", 0);
            yield return ("LoadOnly", 0);
            yield return ("LogArea", 0);
            yield return ("LogAreaBackdrop", 0);
            yield return ("LogAreaScrollBar", 0);
            yield return ("LogBackdrop", 0);
            yield return ("LogDialog", 0);
            yield return ("LogOkButton", 0);
            yield return ("LogOkButtonText", 0);
            yield return ("LogTitle", 0);
            yield return ("LowLatencyLabel", 0);
            yield return ("LowLatencyRadio", 0);
            yield return ("LumberBackdrop", 0);
            yield return ("LumberBackdrop", 1);
            yield return ("LumberBackdrop", 2);
            yield return ("LumberBackdrop", 3);
            yield return ("LumberBackdrop", 4);
            yield return ("LumberBackdrop", 5);
            yield return ("LumberBackdrop", 6);
            yield return ("LumberBackdrop", 7);
            yield return ("LumberBackdrop", 8);
            yield return ("LumberBackdrop", 9);
            yield return ("LumberBackdrop", 10);
            yield return ("LumberBackdrop", 11);
            yield return ("LumberBackdrop", 12);
            yield return ("LumberBackdrop", 13);
            yield return ("LumberBackdrop", 14);
            yield return ("LumberBackdrop", 15);
            yield return ("LumberBackdrop", 16);
            yield return ("LumberBackdrop", 17);
            yield return ("LumberBackdrop", 18);
            yield return ("LumberBackdrop", 19);
            yield return ("LumberBackdrop", 20);
            yield return ("LumberBackdrop", 21);
            yield return ("LumberBackdrop", 22);
            yield return ("LumberBackdrop", 23);
            yield return ("LumberHeader", 0);
            yield return ("LumberText", 0);
            yield return ("LumberText", 1);
            yield return ("LumberText", 2);
            yield return ("LumberText", 3);
            yield return ("LumberText", 4);
            yield return ("LumberText", 5);
            yield return ("LumberText", 6);
            yield return ("LumberText", 7);
            yield return ("LumberText", 8);
            yield return ("LumberText", 9);
            yield return ("LumberText", 10);
            yield return ("LumberText", 11);
            yield return ("LumberText", 12);
            yield return ("LumberText", 13);
            yield return ("LumberText", 14);
            yield return ("LumberText", 15);
            yield return ("LumberText", 16);
            yield return ("LumberText", 17);
            yield return ("LumberText", 18);
            yield return ("LumberText", 19);
            yield return ("LumberText", 20);
            yield return ("LumberText", 21);
            yield return ("LumberText", 22);
            yield return ("LumberText", 23);
            yield return ("MainPanel", 0);
            yield return ("MapListBoxBackdrop", 0);
            yield return ("MapListScrollBar", 0);
            yield return ("MiniMapAllyButton", 0);
            yield return ("MinimapButtonBar", 0);
            yield return ("MiniMapCreepButton", 0);
            yield return ("MiniMapFrame", 0);
            yield return ("MinimapSignalButton", 0);
            yield return ("MiniMapTerrainButton", 0);
            yield return ("ModelDetailLabel", 0);
            yield return ("ModelDetailValue", 0);
            yield return ("MouseScrollDisable", 0);
            yield return ("MouseScrollDisableLabel", 0);
            yield return ("MouseScrollFastLabel", 0);
            yield return ("MouseScrollLabel", 0);
            yield return ("MouseScrollSlider", 0);
            yield return ("MouseScrollSlowLabel", 0);
            yield return ("MovementCheckBox", 0);
            yield return ("MovementLabel", 0);
            yield return ("MusicCheckBox", 0);
            yield return ("MusicVolumeHighLabel", 0);
            yield return ("MusicVolumeLabel", 0);
            yield return ("MusicVolumeLowLabel", 0);
            yield return ("MusicVolumeSlider", 0);
            yield return ("NetworkButton", 0);
            yield return ("NetworkButtonText", 0);
            yield return ("NetworkLabel", 0);
            yield return ("NetworkPanel", 0);
            yield return ("NetworkTitleText", 0);
            yield return ("OcclusionLabel", 0);
            yield return ("OKButtonText", 0);
            yield return ("OptionsButton", 0);
            yield return ("OptionsButtonText", 0);
            yield return ("OptionsCancelButton", 0);
            yield return ("OptionsOKButton", 0);
            yield return ("OptionsPanel", 0);
            yield return ("OptionsPreviousButton", 0);
            yield return ("OptionsPreviousButtonText", 0);
            yield return ("OptionsTitleText", 0);
            yield return ("OverwriteCancelButton", 0);
            yield return ("OverwriteCancelButtonText", 0);
            yield return ("OverwriteMessageText", 0);
            yield return ("OverwriteOnly", 0);
            yield return ("OverwriteOverwriteButton", 0);
            yield return ("OverwriteOverwriteButtonText", 0);
            yield return ("OverwriteTitleText", 0);
            yield return ("ParticlesLabel", 0);
            yield return ("PauseButton", 0);
            yield return ("PauseButtonText", 0);
            yield return ("PlayerNameLabel", 0);
            yield return ("PlayerNameLabel", 1);
            yield return ("PlayerNameLabel", 2);
            yield return ("PlayerNameLabel", 3);
            yield return ("PlayerNameLabel", 4);
            yield return ("PlayerNameLabel", 5);
            yield return ("PlayerNameLabel", 6);
            yield return ("PlayerNameLabel", 7);
            yield return ("PlayerNameLabel", 8);
            yield return ("PlayerNameLabel", 9);
            yield return ("PlayerNameLabel", 10);
            yield return ("PlayerNameLabel", 11);
            yield return ("PlayerNameLabel", 12);
            yield return ("PlayerNameLabel", 13);
            yield return ("PlayerNameLabel", 14);
            yield return ("PlayerNameLabel", 15);
            yield return ("PlayerNameLabel", 16);
            yield return ("PlayerNameLabel", 17);
            yield return ("PlayerNameLabel", 18);
            yield return ("PlayerNameLabel", 19);
            yield return ("PlayerNameLabel", 20);
            yield return ("PlayerNameLabel", 21);
            yield return ("PlayerNameLabel", 22);
            yield return ("PlayerNameLabel", 23);
            yield return ("PlayersHeader", 0);
            yield return ("PositionalCheckBox", 0);
            yield return ("PositionalLabel", 0);
            yield return ("PreviousButton", 0);
            yield return ("PreviousButtonText", 0);
            yield return ("ProviderLabel", 0);
            yield return ("ProviderValue", 0);
            yield return ("QuitButton", 0);
            yield return ("QuitButtonText", 0);
            yield return ("ResolutionLabel", 0);
            yield return ("ResourceBarFrame", 0);
            yield return ("ResourceBarGoldText", 0);
            yield return ("ResourceBarLumberText", 0);
            yield return ("ResourceBarSupplyText", 0);
            yield return ("ResourceBarUpkeepText", 0);
            yield return ("ResourceTradingTitle", 0);
            yield return ("RestartButton", 0);
            yield return ("RestartButtonText", 0);
            yield return ("ReturnButton", 0);
            yield return ("ReturnButtonText", 0);
            yield return ("SaveAndLoad", 0);
            yield return ("SaveGameButton", 0);
            yield return ("SaveGameButtonText", 0);
            yield return ("SaveGameCancelButton", 0);
            yield return ("SaveGameCancelButtonText", 0);
            yield return ("SaveGameDeleteButton", 0);
            yield return ("SaveGameDeleteButtonText", 0);
            yield return ("SaveGameFileEditBox", 0);
            yield return ("SaveGameFileEditBoxText", 0);
            yield return ("SaveGameSaveButton", 0);
            yield return ("SaveGameSaveButtonText", 0);
            yield return ("SaveGameTitleText", 0);
            yield return ("SaveOnly", 0);
            yield return ("ShadowsLabel", 0);
            yield return ("SimpleBuildingActionLabel", 0);
            yield return ("SimpleBuildingActionLabel", 1);
            yield return ("SimpleBuildingDescriptionValue", 1);
            yield return ("SimpleBuildingNameValue", 1);
            yield return ("SimpleBuildQueueBackdrop", 1);
            yield return ("SimpleBuildTimeIndicator", 0);
            yield return ("SimpleBuildTimeIndicator", 1);
            yield return ("SimpleClassValue", 0);
            yield return ("SimpleDestructableNameValue", 4);
            yield return ("SimpleHeroLevelBar", 0);
            yield return ("SimpleHoldDescriptionValue", 2);
            yield return ("SimpleHoldNameValue", 2);
            yield return ("SimpleInfoPanelBuildingDetail", 1);
            yield return ("SimpleInfoPanelCargoDetail", 2);
            yield return ("SimpleInfoPanelDestructableDetail", 4);
            yield return ("SimpleInfoPanelIconAlly", 7);
            yield return ("SimpleInfoPanelIconArmor", 2);
            yield return ("SimpleInfoPanelIconDamage", 0);
            yield return ("SimpleInfoPanelIconDamage", 1);
            yield return ("SimpleInfoPanelIconFood", 4);
            yield return ("SimpleInfoPanelIconGold", 5);
            yield return ("SimpleInfoPanelIconHero", 6);
            yield return ("SimpleInfoPanelIconHeroText", 6);
            yield return ("SimpleInfoPanelIconRank", 3);
            yield return ("SimpleInfoPanelItemDetail", 3);
            yield return ("SimpleInfoPanelUnitDetail", 0);
            yield return ("SimpleInventoryBar", 0);
            yield return ("SimpleInventoryCover", 0);
            yield return ("SimpleItemDescriptionValue", 3);
            yield return ("SimpleItemNameValue", 3);
            yield return ("SimpleNameValue", 0);
            yield return ("SimpleProgressIndicator", 0);
            yield return ("SimpleUnitStatsPanel", 0);
            yield return ("SoundButton", 0);
            yield return ("SoundButtonText", 0);
            yield return ("SoundCheckBox", 0);
            yield return ("SoundPanel", 0);
            yield return ("SoundTitleText", 0);
            yield return ("SoundVolumeHighLabel", 0);
            yield return ("SoundVolumeLabel", 0);
            yield return ("SoundVolumeLowLabel", 0);
            yield return ("SoundVolumeSlider", 0);
            yield return ("SubgroupCheckBox", 0);
            yield return ("SubgroupLabel", 0);
            yield return ("SubtitlesCheckBox", 0);
            yield return ("SubtitlesLabel", 0);
            yield return ("TextureQualityLabel", 0);
            yield return ("TextureQualityValue", 0);
            yield return ("TipsBackButton", 0);
            yield return ("TipsBackButtonText", 0);
            yield return ("TipsButton", 0);
            yield return ("TipsButtonText", 0);
            yield return ("TipsNextButton", 0);
            yield return ("TipsNextButtonText", 0);
            yield return ("TipsOKButton", 0);
            yield return ("TipsOKButtonText", 0);
            yield return ("TipsPanel", 0);
            yield return ("TipsTextArea", 0);
            yield return ("TipsTitleText", 0);
            yield return ("TooltipsCheckBox", 0);
            yield return ("TooltipsLabel", 0);
            yield return ("UnitCheckBox", 0);
            yield return ("UnitLabel", 0);
            yield return ("UnitsCheckBox", 0);
            yield return ("UnitsCheckBox", 1);
            yield return ("UnitsCheckBox", 2);
            yield return ("UnitsCheckBox", 3);
            yield return ("UnitsCheckBox", 4);
            yield return ("UnitsCheckBox", 5);
            yield return ("UnitsCheckBox", 6);
            yield return ("UnitsCheckBox", 7);
            yield return ("UnitsCheckBox", 8);
            yield return ("UnitsCheckBox", 9);
            yield return ("UnitsCheckBox", 10);
            yield return ("UnitsCheckBox", 11);
            yield return ("UnitsCheckBox", 12);
            yield return ("UnitsCheckBox", 13);
            yield return ("UnitsCheckBox", 14);
            yield return ("UnitsCheckBox", 15);
            yield return ("UnitsCheckBox", 16);
            yield return ("UnitsCheckBox", 17);
            yield return ("UnitsCheckBox", 18);
            yield return ("UnitsCheckBox", 19);
            yield return ("UnitsCheckBox", 20);
            yield return ("UnitsCheckBox", 21);
            yield return ("UnitsCheckBox", 22);
            yield return ("UnitsCheckBox", 23);
            yield return ("UnitsHeader", 0);
            yield return ("UpperButtonBarAlliesButton", 0);
            yield return ("UpperButtonBarChatButton", 0);
            yield return ("UpperButtonBarFrame", 0);
            yield return ("UpperButtonBarMenuButton", 0);
            yield return ("UpperButtonBarQuestsButton", 0);
            yield return ("VideoButton", 0);
            yield return ("VideoButtonText", 0);
            yield return ("VideoPanel", 0);
            yield return ("VideoTitleText", 0);
            yield return ("VisionCheckBox", 0);
            yield return ("VisionCheckBox", 1);
            yield return ("VisionCheckBox", 2);
            yield return ("VisionCheckBox", 3);
            yield return ("VisionCheckBox", 4);
            yield return ("VisionCheckBox", 5);
            yield return ("VisionCheckBox", 6);
            yield return ("VisionCheckBox", 7);
            yield return ("VisionCheckBox", 8);
            yield return ("VisionCheckBox", 9);
            yield return ("VisionCheckBox", 10);
            yield return ("VisionCheckBox", 11);
            yield return ("VisionCheckBox", 12);
            yield return ("VisionCheckBox", 13);
            yield return ("VisionCheckBox", 14);
            yield return ("VisionCheckBox", 15);
            yield return ("VisionCheckBox", 16);
            yield return ("VisionCheckBox", 17);
            yield return ("VisionCheckBox", 18);
            yield return ("VisionCheckBox", 19);
            yield return ("VisionCheckBox", 20);
            yield return ("VisionCheckBox", 21);
            yield return ("VisionCheckBox", 22);
            yield return ("VisionCheckBox", 23);
            yield return ("VisionHeader", 0);
            yield return ("VSyncCheckBox", 0);
            yield return ("VSyncLabel", 0);
            yield return ("WindowModeLabel", 0);
            yield return ("WouldTheRealOptionsTitleTextPleaseStandUp", 0);
        }
    }
}