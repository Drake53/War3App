﻿using War3Net.Build.Audio;
using War3Net.Build.Environment;
using War3Net.Build.Info;
using War3Net.Build.Object;
using War3Net.Build.Widget;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.WinForms.Extensions
{
    public static class MpqArchiveExtensions
    {
        public static void AddFileNames(this MpqArchive archive)
        {
            if (archive.IsCampaignArchive(out var campaignInfo))
            {
                for (var i = 0; i < campaignInfo.MapCount; i++)
                {
                    archive.AddFilename(campaignInfo.GetMap(i).MapFilePath);
                }
            }

            archive.AddFilename("(attributes)");
            archive.AddFilename("conversation.json");

            archive.AddFilename(MapSounds.FileName);
            archive.AddFilename(MapInfo.FileName);
            archive.AddFilename(MapDoodads.FileName);
            archive.AddFilename(MapUnits.FileName);

            archive.AddFilename(MapEnvironment.FileName);
            archive.AddFilename(MapPreviewIcons.FileName);
            archive.AddFilename(MapRegions.FileName);

            archive.AddFilename(MapUnitObjectData.FileName);
            archive.AddFilename(MapItemObjectData.FileName);
            archive.AddFilename(MapDestructableObjectData.FileName);
            archive.AddFilename(MapDoodadObjectData.FileName);
            archive.AddFilename(MapAbilityObjectData.FileName);
            archive.AddFilename(MapBuffObjectData.FileName);
            archive.AddFilename(MapUpgradeObjectData.FileName);

            archive.AddFilename(CampaignInfo.FileName);

            archive.AddFilename(CampaignUnitObjectData.FileName);
            archive.AddFilename(CampaignItemObjectData.FileName);
            archive.AddFilename(CampaignDestructableObjectData.FileName);
            archive.AddFilename(CampaignDoodadObjectData.FileName);
            archive.AddFilename(CampaignAbilityObjectData.FileName);
            archive.AddFilename(CampaignBuffObjectData.FileName);
            archive.AddFilename(CampaignUpgradeObjectData.FileName);

            archive.AddFilename(@"war3map.j");
            archive.AddFilename(@"scripts\war3map.j");
            archive.AddFilename(@"war3map.lua");
        }

        public static bool IsCampaignArchive(this MpqArchive archive, out CampaignInfo campaignInfo)
        {
            if (archive.TryAddFilename(CampaignInfo.FileName))
            {
                using var campaignInfoFileStream = archive.OpenFile(CampaignInfo.FileName);
                campaignInfo = CampaignInfo.Parse(campaignInfoFileStream);
                return true;
            }

            campaignInfo = null;
            return false;
        }
    }
}