using System;
using System.Collections.Generic;

using War3Net.Build.Common;
using War3Net.Common.Extensions;

namespace War3App.MapAdapter.Object
{
    public static class UpgradeObjectDataProvider
    {
        public static IEnumerable<int> GetRawcodes(GamePatch patch)
        {
            switch (patch)
            {
                case GamePatch.v1_29_0:
                case GamePatch.v1_29_1:
                case GamePatch.v1_29_2:
                    return GetRawcodesPatch1_29_2();

                case GamePatch.v1_30_0:
                case GamePatch.v1_30_1:
                case GamePatch.v1_30_2:
                case GamePatch.v1_30_3:
                case GamePatch.v1_30_4:
                    // TODO
                    return GetRawcodesPatch1_29_2();

                case GamePatch.v1_31_0:
                case GamePatch.v1_31_1:
                    return GetRawcodesPatch1_31_1_12173();

                default:
                    throw new NotImplementedException();
            }
        }

        public static IEnumerable<int> GetPropertyRawcodes(GamePatch patch)
        {
            switch (patch)
            {
                case GamePatch.v1_29_0:
                case GamePatch.v1_29_1:
                case GamePatch.v1_29_2:
                    return GetPropertyRawcodesPatch1_29_2();

                case GamePatch.v1_30_0:
                case GamePatch.v1_30_1:
                case GamePatch.v1_30_2:
                case GamePatch.v1_30_3:
                case GamePatch.v1_30_4:
                    // TODO
                    return GetPropertyRawcodesPatch1_29_2();

                case GamePatch.v1_31_0:
                case GamePatch.v1_31_1:
                    return GetPropertyRawcodesPatch1_31_1_12173();

                default:
                    throw new NotImplementedException();
            }
        }

        private static IEnumerable<int> GetRawcodesPatch1_29_2()
        {
            yield return "Rhme".FromRawcode();
            yield return "Rhra".FromRawcode();
            yield return "Rhhb".FromRawcode();
            yield return "Rhar".FromRawcode();
            yield return "Rhgb".FromRawcode();
            yield return "Rhac".FromRawcode();
            yield return "Rhde".FromRawcode();
            yield return "Rhan".FromRawcode();
            yield return "Rhpt".FromRawcode();
            yield return "Rhst".FromRawcode();
            yield return "Rhla".FromRawcode();
            yield return "Rhri".FromRawcode();
            yield return "Rhlh".FromRawcode();
            yield return "Rhse".FromRawcode();
            yield return "Rhfl".FromRawcode();
            yield return "Rhss".FromRawcode();
            yield return "Rhrt".FromRawcode();
            yield return "Rhpm".FromRawcode();
            yield return "Rhfc".FromRawcode();
            yield return "Rhfs".FromRawcode();
            yield return "Rhcd".FromRawcode();
            yield return "Rome".FromRawcode();
            yield return "Rora".FromRawcode();
            yield return "Roar".FromRawcode();
            yield return "Rwdm".FromRawcode();
            yield return "Ropg".FromRawcode();
            yield return "Robs".FromRawcode();
            yield return "Rows".FromRawcode();
            yield return "Roen".FromRawcode();
            yield return "Rovs".FromRawcode();
            yield return "Rowd".FromRawcode();
            yield return "Rost".FromRawcode();
            yield return "Rosp".FromRawcode();
            yield return "Rotr".FromRawcode();
            yield return "Rolf".FromRawcode();
            yield return "Roch".FromRawcode();
            yield return "Rowt".FromRawcode();
            yield return "Rorb".FromRawcode();
            yield return "Robk".FromRawcode();
            yield return "Ropm".FromRawcode();
            yield return "Robf".FromRawcode();
            yield return "Rume".FromRawcode();
            yield return "Rura".FromRawcode();
            yield return "Ruar".FromRawcode();
            yield return "Ruac".FromRawcode();
            yield return "Rugf".FromRawcode();
            yield return "Ruwb".FromRawcode();
            yield return "Rusf".FromRawcode();
            yield return "Rune".FromRawcode();
            yield return "Ruba".FromRawcode();
            yield return "Rufb".FromRawcode();
            yield return "Rusl".FromRawcode();
            yield return "Rucr".FromRawcode();
            yield return "Rupc".FromRawcode();
            yield return "Rusm".FromRawcode();
            yield return "Rubu".FromRawcode();
            yield return "Rusp".FromRawcode();
            yield return "Ruex".FromRawcode();
            yield return "Rupm".FromRawcode();
            yield return "Resm".FromRawcode();
            yield return "Resw".FromRawcode();
            yield return "Rema".FromRawcode();
            yield return "Rerh".FromRawcode();
            yield return "Reuv".FromRawcode();
            yield return "Renb".FromRawcode();
            yield return "Resc".FromRawcode();
            yield return "Remg".FromRawcode();
            yield return "Reib".FromRawcode();
            yield return "Remk".FromRawcode();
            yield return "Redt".FromRawcode();
            yield return "Redc".FromRawcode();
            yield return "Resi".FromRawcode();
            yield return "Recb".FromRawcode();
            yield return "Reht".FromRawcode();
            yield return "Repb".FromRawcode();
            yield return "Rers".FromRawcode();
            yield return "Rehs".FromRawcode();
            yield return "Reeb".FromRawcode();
            yield return "Reec".FromRawcode();
            yield return "Rews".FromRawcode();
            yield return "Repm".FromRawcode();
            yield return "Rgfo".FromRawcode();
            yield return "Rguv".FromRawcode();
            yield return "Rnen".FromRawcode();
            yield return "Rnsw".FromRawcode();
            yield return "Rnsi".FromRawcode();
            yield return "Rnat".FromRawcode();
            yield return "Rnam".FromRawcode();
            yield return "Rnsb".FromRawcode();
        }

        private static IEnumerable<int> GetRawcodesPatch1_31_1_12173()
        {
            yield return "Rhme".FromRawcode();
            yield return "Rhra".FromRawcode();
            yield return "Rhhb".FromRawcode();
            yield return "Rhar".FromRawcode();
            yield return "Rhgb".FromRawcode();
            yield return "Rhac".FromRawcode();
            yield return "Rhde".FromRawcode();
            yield return "Rhan".FromRawcode();
            yield return "Rhpt".FromRawcode();
            yield return "Rhst".FromRawcode();
            yield return "Rhla".FromRawcode();
            yield return "Rhri".FromRawcode();
            yield return "Rhlh".FromRawcode();
            yield return "Rhse".FromRawcode();
            yield return "Rhfl".FromRawcode();
            yield return "Rhss".FromRawcode();
            yield return "Rhrt".FromRawcode();
            yield return "Rhpm".FromRawcode();
            yield return "Rhfc".FromRawcode();
            yield return "Rhfs".FromRawcode();
            yield return "Rhcd".FromRawcode();
            yield return "Rhsb".FromRawcode();
            yield return "Rome".FromRawcode();
            yield return "Rora".FromRawcode();
            yield return "Roar".FromRawcode();
            yield return "Rwdm".FromRawcode();
            yield return "Ropg".FromRawcode();
            yield return "Robs".FromRawcode();
            yield return "Rows".FromRawcode();
            yield return "Roen".FromRawcode();
            yield return "Rovs".FromRawcode();
            yield return "Rowd".FromRawcode();
            yield return "Rost".FromRawcode();
            yield return "Rosp".FromRawcode();
            yield return "Rotr".FromRawcode();
            yield return "Rolf".FromRawcode();
            yield return "Roch".FromRawcode();
            yield return "Rowt".FromRawcode();
            yield return "Rorb".FromRawcode();
            yield return "Robk".FromRawcode();
            yield return "Ropm".FromRawcode();
            yield return "Robf".FromRawcode();
            yield return "Rume".FromRawcode();
            yield return "Rura".FromRawcode();
            yield return "Ruar".FromRawcode();
            yield return "Ruac".FromRawcode();
            yield return "Rugf".FromRawcode();
            yield return "Ruwb".FromRawcode();
            yield return "Rusf".FromRawcode();
            yield return "Rune".FromRawcode();
            yield return "Ruba".FromRawcode();
            yield return "Rufb".FromRawcode();
            yield return "Rusl".FromRawcode();
            yield return "Rucr".FromRawcode();
            yield return "Rupc".FromRawcode();
            yield return "Rusm".FromRawcode();
            yield return "Rubu".FromRawcode();
            yield return "Rusp".FromRawcode();
            yield return "Ruex".FromRawcode();
            yield return "Rupm".FromRawcode();
            yield return "Resm".FromRawcode();
            yield return "Resw".FromRawcode();
            yield return "Rema".FromRawcode();
            yield return "Rerh".FromRawcode();
            yield return "Reuv".FromRawcode();
            yield return "Renb".FromRawcode();
            yield return "Resc".FromRawcode();
            yield return "Remg".FromRawcode();
            yield return "Reib".FromRawcode();
            yield return "Remk".FromRawcode();
            yield return "Redt".FromRawcode();
            yield return "Redc".FromRawcode();
            yield return "Resi".FromRawcode();
            yield return "Recb".FromRawcode();
            yield return "Reht".FromRawcode();
            yield return "Repb".FromRawcode();
            yield return "Rers".FromRawcode();
            yield return "Rehs".FromRawcode();
            yield return "Reeb".FromRawcode();
            yield return "Reec".FromRawcode();
            yield return "Rews".FromRawcode();
            yield return "Repm".FromRawcode();
            yield return "Rgfo".FromRawcode();
            yield return "Rguv".FromRawcode();
            yield return "Rnen".FromRawcode();
            yield return "Rnsw".FromRawcode();
            yield return "Rnsi".FromRawcode();
            yield return "Rnat".FromRawcode();
            yield return "Rnam".FromRawcode();
            yield return "Rnsb".FromRawcode();
        }

        private static IEnumerable<int> GetPropertyRawcodesPatch1_29_2()
        {
            yield return "gnam".FromRawcode();
            yield return "gnsf".FromRawcode();
            yield return "grac".FromRawcode();
            yield return "gtp1".FromRawcode();
            yield return "gub1".FromRawcode();
            yield return "ghk1".FromRawcode();
            yield return "gbpx".FromRawcode();
            yield return "gbpy".FromRawcode();
            yield return "gar1".FromRawcode();
            yield return "gcls".FromRawcode();
            yield return "glvl".FromRawcode();
            yield return "gglb".FromRawcode();
            yield return "gglm".FromRawcode();
            yield return "glmb".FromRawcode();
            yield return "glmm".FromRawcode();
            yield return "gtib".FromRawcode();
            yield return "gtim".FromRawcode();
            yield return "gef1".FromRawcode();
            yield return "gba1".FromRawcode();
            yield return "gmo1".FromRawcode();
            yield return "gco1".FromRawcode();
            yield return "gef2".FromRawcode();
            yield return "gba2".FromRawcode();
            yield return "gmo2".FromRawcode();
            yield return "gco2".FromRawcode();
            yield return "gef3".FromRawcode();
            yield return "gba3".FromRawcode();
            yield return "gmo3".FromRawcode();
            yield return "gco3".FromRawcode();
            yield return "gef4".FromRawcode();
            yield return "gba4".FromRawcode();
            yield return "gmo4".FromRawcode();
            yield return "gco4".FromRawcode();
            yield return "ginh".FromRawcode();
            yield return "greq".FromRawcode();
            yield return "grqc".FromRawcode();
            yield return "glob".FromRawcode();
        }

        private static IEnumerable<int> GetPropertyRawcodesPatch1_31_1_12173()
        {
            yield return "gnam".FromRawcode();
            yield return "gnsf".FromRawcode();
            yield return "grac".FromRawcode();
            yield return "gtp1".FromRawcode();
            yield return "gub1".FromRawcode();
            yield return "ghk1".FromRawcode();
            yield return "gbpx".FromRawcode();
            yield return "gbpy".FromRawcode();
            yield return "gar1".FromRawcode();
            yield return "gcls".FromRawcode();
            yield return "glvl".FromRawcode();
            yield return "gglb".FromRawcode();
            yield return "gglm".FromRawcode();
            yield return "glmb".FromRawcode();
            yield return "glmm".FromRawcode();
            yield return "gtib".FromRawcode();
            yield return "gtim".FromRawcode();
            yield return "gef1".FromRawcode();
            yield return "gba1".FromRawcode();
            yield return "gmo1".FromRawcode();
            yield return "gco1".FromRawcode();
            yield return "gef2".FromRawcode();
            yield return "gba2".FromRawcode();
            yield return "gmo2".FromRawcode();
            yield return "gco2".FromRawcode();
            yield return "gef3".FromRawcode();
            yield return "gba3".FromRawcode();
            yield return "gmo3".FromRawcode();
            yield return "gco3".FromRawcode();
            yield return "gef4".FromRawcode();
            yield return "gba4".FromRawcode();
            yield return "gmo4".FromRawcode();
            yield return "gco4".FromRawcode();
            yield return "ginh".FromRawcode();
            yield return "greq".FromRawcode();
            yield return "grqc".FromRawcode();
            yield return "glob".FromRawcode();
        }
    }
}