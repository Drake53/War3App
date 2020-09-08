using System;
using System.Collections.Generic;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Script
{
    public static class CommonTypesProvider
    {
        public static IEnumerable<string> GetTypes(GamePatch from, GamePatch to)
        {
            for (var patch = from + 1; patch <= to; patch++)
            {
                foreach (var identifier in GetTypes(patch))
                {
                    yield return identifier;
                }
            }
        }

        public static IEnumerable<string> GetTypes(GamePatch patch)
        {
            return patch switch
            {
                GamePatch.v1_24a => GetTypesPatch1_24a(),
                GamePatch.v1_29_0 => GetTypesPatch1_29_0(),
                GamePatch.v1_30_0 => GetTypesPatch1_30_0(),
                GamePatch.v1_31_0 => GetTypesPatch1_31_0(),
                GamePatch.v1_32_0 => GetTypesPatch1_32_0(),

                _ => Array.Empty<string>(),
            };
        }

        private static IEnumerable<string> GetTypesPatch1_24a()
        {
            yield return nameof(War3Api.Common.agent);
            yield return nameof(War3Api.Common.hashtable);
        }

        private static IEnumerable<string> GetTypesPatch1_29_0()
        {
            yield return nameof(War3Api.Common.mousebuttontype);
        }

        private static IEnumerable<string> GetTypesPatch1_30_0()
        {
            yield return nameof(War3Api.Common.animtype);
            yield return nameof(War3Api.Common.subanimtype);
        }

        private static IEnumerable<string> GetTypesPatch1_31_0()
        {
            yield return nameof(War3Api.Common.framehandle);
            yield return nameof(War3Api.Common.originframetype);
            yield return nameof(War3Api.Common.framepointtype);
            yield return nameof(War3Api.Common.textaligntype);
            yield return nameof(War3Api.Common.frameeventtype);
            yield return nameof(War3Api.Common.oskeytype);

            yield return nameof(War3Api.Common.abilityintegerfield);
            yield return nameof(War3Api.Common.abilityrealfield);
            yield return nameof(War3Api.Common.abilitybooleanfield);
            yield return nameof(War3Api.Common.abilitystringfield);
            yield return nameof(War3Api.Common.abilityintegerlevelfield);
            yield return nameof(War3Api.Common.abilityreallevelfield);
            yield return nameof(War3Api.Common.abilitybooleanlevelfield);
            yield return nameof(War3Api.Common.abilitystringlevelfield);
            yield return nameof(War3Api.Common.abilityintegerlevelarrayfield);
            yield return nameof(War3Api.Common.abilityreallevelarrayfield);
            yield return nameof(War3Api.Common.abilitybooleanlevelarrayfield);
            yield return nameof(War3Api.Common.abilitystringlevelarrayfield);

            yield return nameof(War3Api.Common.unitintegerfield);
            yield return nameof(War3Api.Common.unitrealfield);
            yield return nameof(War3Api.Common.unitbooleanfield);
            yield return nameof(War3Api.Common.unitstringfield);
            yield return nameof(War3Api.Common.unitweaponintegerfield);
            yield return nameof(War3Api.Common.unitweaponrealfield);
            yield return nameof(War3Api.Common.unitweaponbooleanfield);
            yield return nameof(War3Api.Common.unitweaponstringfield);

            yield return nameof(War3Api.Common.itemintegerfield);
            yield return nameof(War3Api.Common.itemrealfield);
            yield return nameof(War3Api.Common.itembooleanfield);
            yield return nameof(War3Api.Common.itemstringfield);

            yield return nameof(War3Api.Common.movetype);
            yield return nameof(War3Api.Common.targetflag);
            yield return nameof(War3Api.Common.armortype);
            yield return nameof(War3Api.Common.heroattribute);
            yield return nameof(War3Api.Common.defensetype);
            yield return nameof(War3Api.Common.regentype);
            yield return nameof(War3Api.Common.unitcategory);
            yield return nameof(War3Api.Common.pathingflag);
        }

        private static IEnumerable<string> GetTypesPatch1_32_0()
        {
            yield return nameof(War3Api.Common.minimapicon);
            yield return nameof(War3Api.Common.commandbuttoneffect);
        }
    }
}