using System;
using System.Collections.Generic;

using War3Net.Build.Common;

namespace War3App.MapDowngrader
{
    public static class BuffObjectDataProvider
    {
        public static IEnumerable<int> GetRawcodes(GamePatch patch)
        {
            switch (patch)
            {
                case GamePatch.v1_29_0:
                case GamePatch.v1_29_1:
                case GamePatch.v1_29_2:
                    return GetRawcodesPatch1_29_2();

                case GamePatch.v1_31_0:
                case GamePatch.v1_31_1:
                    // return GetRawcodesPatch1_31_1();
                    return GetRawcodesPatch1_29_2();

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

                case GamePatch.v1_31_0:
                case GamePatch.v1_31_1:
                    // return GetPropertyRawcodesPatch1_31_1();
                    return GetPropertyRawcodesPatch1_29_2();

                default:
                    throw new NotImplementedException();
            }
        }

        private static IEnumerable<int> GetRawcodesPatch1_29_2()
        {
            yield return "BPSE".FromRawcode();
            yield return "BSTN".FromRawcode();
            yield return "Bchd".FromRawcode();
            yield return "Bdef".FromRawcode();
            yield return "Bdet".FromRawcode();
            yield return "Bfre".FromRawcode();
            yield return "Bfro".FromRawcode();
            yield return "Bvul".FromRawcode();
            yield return "Bpoi".FromRawcode();
            yield return "Bpsd".FromRawcode();
            yield return "Bpsi".FromRawcode();
            yield return "Bsha".FromRawcode();
            yield return "Bspe".FromRawcode();
            yield return "Btrv".FromRawcode();
            yield return "Bclf".FromRawcode();
            yield return "Bcmg".FromRawcode();
            yield return "Bhea".FromRawcode();
            yield return "Binf".FromRawcode();
            yield return "Binv".FromRawcode();
            yield return "Bmlc".FromRawcode();
            yield return "Bmlt".FromRawcode();
            yield return "Bmil".FromRawcode();
            yield return "Bpxf".FromRawcode();
            yield return "Bphx".FromRawcode();
            yield return "Bply".FromRawcode();
            yield return "Bslo".FromRawcode();
            yield return "BHab".FromRawcode();
            yield return "BHad".FromRawcode();
            yield return "BHav".FromRawcode();
            yield return "BHbn".FromRawcode();
            yield return "BHbd".FromRawcode();
            yield return "BHbz".FromRawcode();
            yield return "BHds".FromRawcode();
            yield return "Bdcb".FromRawcode();
            yield return "Bdcl".FromRawcode();
            yield return "Bdcm".FromRawcode();
            yield return "Bdtb".FromRawcode();
            yield return "Bdtl".FromRawcode();
            yield return "Bdtm".FromRawcode();
            yield return "BHfs".FromRawcode();
            yield return "BHtc".FromRawcode();
            yield return "BHwe".FromRawcode();
            yield return "Bakb".FromRawcode();
            yield return "Boar".FromRawcode();
            yield return "Barm".FromRawcode();
            yield return "Bbof".FromRawcode();
            yield return "Bbsk".FromRawcode();
            yield return "Bblo".FromRawcode();
            yield return "Bdvv".FromRawcode();
            yield return "Bdig".FromRawcode();
            yield return "Bens".FromRawcode();
            yield return "Bena".FromRawcode();
            yield return "Beng".FromRawcode();
            yield return "Beye".FromRawcode();
            yield return "Bhwd".FromRawcode();
            yield return "Blsh".FromRawcode();
            yield return "Blsa".FromRawcode();
            yield return "Bliq".FromRawcode();
            yield return "Bprg".FromRawcode();
            yield return "Bspl".FromRawcode();
            yield return "Bstt".FromRawcode();
            yield return "BOac".FromRawcode();
            yield return "BOae".FromRawcode();
            yield return "BOeq".FromRawcode();
            yield return "BOea".FromRawcode();
            yield return "BOhx".FromRawcode();
            yield return "BOmi".FromRawcode();
            yield return "BOsh".FromRawcode();
            yield return "BOsf".FromRawcode();
            yield return "BOvd".FromRawcode();
            yield return "BOvc".FromRawcode();
            yield return "BOwd".FromRawcode();
            yield return "BOww".FromRawcode();
            yield return "BOwk".FromRawcode();
            yield return "Bbar".FromRawcode();
            yield return "Bcor".FromRawcode();
            yield return "Bcyc".FromRawcode();
            yield return "Bcy2".FromRawcode();
            yield return "Beat".FromRawcode();
            yield return "Bfae".FromRawcode();
            yield return "Bgra".FromRawcode();
            yield return "Bmfl".FromRawcode();
            yield return "Bmfa".FromRawcode();
            yield return "Bpsh".FromRawcode();
            yield return "Brej".FromRawcode();
            yield return "Broa".FromRawcode();
            yield return "Bspo".FromRawcode();
            yield return "Bssd".FromRawcode();
            yield return "Bssi".FromRawcode();
            yield return "Bvng".FromRawcode();
            yield return "BEah".FromRawcode();
            yield return "BEar".FromRawcode();
            yield return "BEer".FromRawcode();
            yield return "BEfn".FromRawcode();
            yield return "BEim".FromRawcode();
            yield return "BEia".FromRawcode();
            yield return "BEme".FromRawcode();
            yield return "BEst".FromRawcode();
            yield return "BEsh".FromRawcode();
            yield return "BEsv".FromRawcode();
            yield return "Bams".FromRawcode();
            yield return "Bam2".FromRawcode();
            yield return "Babr".FromRawcode();
            yield return "Bapl".FromRawcode();
            yield return "Bcri".FromRawcode();
            yield return "Bcrs".FromRawcode();
            yield return "Bfrz".FromRawcode();
            yield return "Bplg".FromRawcode();
            yield return "Bpoc".FromRawcode();
            yield return "Bpos".FromRawcode();
            yield return "Brai".FromRawcode();
            yield return "Brpb".FromRawcode();
            yield return "Brpl".FromRawcode();
            yield return "Brpm".FromRawcode();
            yield return "Bspa".FromRawcode();
            yield return "Buhf".FromRawcode();
            yield return "Buns".FromRawcode();
            yield return "Bweb".FromRawcode();
            yield return "Bwea".FromRawcode();
            yield return "BUan".FromRawcode();
            yield return "BUau".FromRawcode();
            yield return "BUav".FromRawcode();
            yield return "BUcb".FromRawcode();
            yield return "BUcs".FromRawcode();
            yield return "BUdd".FromRawcode();
            yield return "BUfa".FromRawcode();
            yield return "BUim".FromRawcode();
            yield return "BUsl".FromRawcode();
            yield return "BUsp".FromRawcode();
            yield return "BUst".FromRawcode();
            yield return "BUts".FromRawcode();
            yield return "Basl".FromRawcode();
            yield return "BCbf".FromRawcode();
            yield return "BCtc".FromRawcode();
            yield return "Bfzy".FromRawcode();
            yield return "Bmec".FromRawcode();
            yield return "BNmr".FromRawcode();
            yield return "Bpig".FromRawcode();
            yield return "BNpi".FromRawcode();
            yield return "BNsa".FromRawcode();
            yield return "Bshs".FromRawcode();
            yield return "BNss".FromRawcode();
            yield return "Btdg".FromRawcode();
            yield return "Btsp".FromRawcode();
            yield return "Btsa".FromRawcode();
            yield return "BNba".FromRawcode();
            yield return "BNbf".FromRawcode();
            yield return "BHca".FromRawcode();
            yield return "Bcsd".FromRawcode();
            yield return "Bcsi".FromRawcode();
            yield return "BNdm".FromRawcode();
            yield return "BNdo".FromRawcode();
            yield return "BNdi".FromRawcode();
            yield return "BNdh".FromRawcode();
            yield return "BNef".FromRawcode();
            yield return "BNht".FromRawcode();
            yield return "BNms".FromRawcode();
            yield return "BNsi".FromRawcode();
            yield return "BNst".FromRawcode();
            yield return "BNsg".FromRawcode();
            yield return "BNsq".FromRawcode();
            yield return "BNsw".FromRawcode();
            yield return "BNto".FromRawcode();
            yield return "BNwm".FromRawcode();
            yield return "BNbr".FromRawcode();
            yield return "BNdc".FromRawcode();
            yield return "BNin".FromRawcode();
            yield return "BNpa".FromRawcode();
            yield return "BNpm".FromRawcode();
            yield return "BNrd".FromRawcode();
            yield return "BNrf".FromRawcode();
            yield return "BNsl".FromRawcode();
            yield return "BIcb".FromRawcode();
            yield return "BFig".FromRawcode();
            yield return "BIcf".FromRawcode();
            yield return "BIil".FromRawcode();
            yield return "BIrb".FromRawcode();
            yield return "BIrg".FromRawcode();
            yield return "BIrl".FromRawcode();
            yield return "BIrm".FromRawcode();
            yield return "BIsv".FromRawcode();
            yield return "BIsh".FromRawcode();
            yield return "BIwb".FromRawcode();
            yield return "BImo".FromRawcode();
            yield return "BIpv".FromRawcode();
            yield return "Xclf".FromRawcode();
            yield return "Xfla".FromRawcode();
            yield return "XHbz".FromRawcode();
            yield return "XHfs".FromRawcode();
            yield return "Xbof".FromRawcode();
            yield return "XOeq".FromRawcode();
            yield return "XOre".FromRawcode();
            yield return "Xesn".FromRawcode();
            yield return "XEsf".FromRawcode();
            yield return "XEtq".FromRawcode();
            yield return "XUdd".FromRawcode();
            yield return "XNmo".FromRawcode();
            yield return "XErc".FromRawcode();
            yield return "XErf".FromRawcode();
            yield return "XIct".FromRawcode();
            yield return "AEsd".FromRawcode();
            yield return "AEtr".FromRawcode();
            yield return "ANmd".FromRawcode();
            yield return "Bivs".FromRawcode();
            yield return "BUad".FromRawcode();
            yield return "Bult".FromRawcode();
            yield return "BNab".FromRawcode();
            yield return "BNcr".FromRawcode();
            yield return "BNhs".FromRawcode();
            yield return "XNhs".FromRawcode();
            yield return "BNtm".FromRawcode();
            yield return "BNeg".FromRawcode();
            yield return "BNcs".FromRawcode();
            yield return "XNcs".FromRawcode();
            yield return "BNfy".FromRawcode();
            yield return "BNcg".FromRawcode();
            yield return "BNic".FromRawcode();
            yield return "BNso".FromRawcode();
            yield return "BNlm".FromRawcode();
            yield return "BNvc".FromRawcode();
            yield return "BNva".FromRawcode();
            yield return "XNvc".FromRawcode();
            yield return "Xbdt".FromRawcode();
            yield return "Xbli".FromRawcode();
            yield return "Xdis".FromRawcode();
            yield return "Xfhs".FromRawcode();
            yield return "Xfhm".FromRawcode();
            yield return "Xfhl".FromRawcode();
            yield return "Xfos".FromRawcode();
            yield return "Xfom".FromRawcode();
            yield return "Xfol".FromRawcode();
            yield return "Xfns".FromRawcode();
            yield return "Xfnm".FromRawcode();
            yield return "Xfnl".FromRawcode();
            yield return "Xfus".FromRawcode();
            yield return "Xfum".FromRawcode();
            yield return "Xful".FromRawcode();
        }

        private static IEnumerable<int> GetPropertyRawcodesPatch1_29_2()
        {
            yield return "fnam".FromRawcode();
            yield return "fnsf".FromRawcode();
            yield return "ftip".FromRawcode();
            yield return "fube".FromRawcode();
            yield return "feff".FromRawcode();
            yield return "frac".FromRawcode();
            yield return "fart".FromRawcode();
            yield return "ftat".FromRawcode();
            yield return "fsat".FromRawcode();
            yield return "feat".FromRawcode();
            yield return "flig".FromRawcode();
            yield return "fmat".FromRawcode();
            yield return "fmsp".FromRawcode();
            yield return "fmac".FromRawcode();
            yield return "fmho".FromRawcode();
            yield return "ftac".FromRawcode();
            yield return "fta0".FromRawcode();
            yield return "fta1".FromRawcode();
            yield return "fta2".FromRawcode();
            yield return "fta3".FromRawcode();
            yield return "fta4".FromRawcode();
            yield return "fta5".FromRawcode();
            yield return "feft".FromRawcode();
            yield return "fspt".FromRawcode();
            yield return "fefs".FromRawcode();
            yield return "fefl".FromRawcode();
            yield return "fspd".FromRawcode();
        }
    }
}