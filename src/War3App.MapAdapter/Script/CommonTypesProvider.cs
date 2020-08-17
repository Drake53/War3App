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
                GamePatch.v1_31_0 => GetTypesPatch1_31_0(),
                GamePatch.v1_32_0 => GetTypesPatch1_32_0(),

                _ => Array.Empty<string>(),
            };
        }

        private static IEnumerable<string> GetTypesPatch1_31_0()
        {
            // TODO
            yield break;
        }

        private static IEnumerable<string> GetTypesPatch1_32_0()
        {
            yield return nameof(War3Api.Common.minimapicon);
            yield return nameof(War3Api.Common.commandbuttoneffect);
        }
    }
}