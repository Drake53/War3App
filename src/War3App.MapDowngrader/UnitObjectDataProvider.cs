using System;
using System.Collections.Generic;

using War3Net.Build.Common;

namespace War3App.MapDowngrader
{
    public static class UnitObjectDataProvider
    {
        public static IEnumerable<int> GetRawcodes(GamePatch from, GamePatch to)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<int> GetRawcodes(GamePatch patch)
        {
            if (patch != GamePatch.v1_31_0)
            {
                throw new NotImplementedException();
            }

            // Melee units
            {
                yield return "hpea".FromRawcode();
                yield return "hfoo".FromRawcode();
                yield return "hkni".FromRawcode();
                yield return "hrif".FromRawcode();
                yield return "hmtm".FromRawcode();
                yield return "hgyr".FromRawcode();
                yield return "hgry".FromRawcode();
                yield return "hmpr".FromRawcode();
                yield return "hsor".FromRawcode();
                yield return "hmtt".FromRawcode();
                yield return "hspt".FromRawcode();
                yield return "hdhw".FromRawcode();

                yield return "opeo".FromRawcode();
                yield return "ogru".FromRawcode();
                yield return "orai".FromRawcode();
                yield return "otau".FromRawcode();
                yield return "ohun".FromRawcode();
                yield return "ocat".FromRawcode();
                yield return "okod".FromRawcode();
                yield return "owyv".FromRawcode();
                yield return "otbr".FromRawcode();
                yield return "odoc".FromRawcode();
                yield return "oshm".FromRawcode();
                yield return "ospw".FromRawcode();

                yield return "uaco".FromRawcode();
                yield return "ushd".FromRawcode();
                yield return "ugho".FromRawcode();
                yield return "uabo".FromRawcode();
                yield return "umtw".FromRawcode();
                yield return "ucry".FromRawcode();
                yield return "ugar".FromRawcode();
                yield return "uban".FromRawcode();
                yield return "unec".FromRawcode();
                yield return "uobs".FromRawcode();
                yield return "ufro".FromRawcode();

                yield return "ewsp".FromRawcode();
                yield return "earc".FromRawcode();
                yield return "esen".FromRawcode();
                yield return "edry".FromRawcode();
                yield return "ebal".FromRawcode();
                yield return "ehip".FromRawcode();
                yield return "ehpr".FromRawcode();
                yield return "echm".FromRawcode();
                yield return "edot".FromRawcode();
                yield return "edoc".FromRawcode();
                yield return "emtg".FromRawcode();
                yield return "efdr".FromRawcode();

                yield return "nalb".FromRawcode();
                yield return "nech".FromRawcode();
                yield return "ncrb".FromRawcode();
                yield return "ndog".FromRawcode();
                yield return "ndwm".FromRawcode();
                yield return "nfbr".FromRawcode();
                yield return "nfro".FromRawcode();
                yield return "nhmc".FromRawcode();
                yield return "npng".FromRawcode();
                yield return "npig".FromRawcode();
                yield return "necr".FromRawcode();
                yield return "nrac".FromRawcode();
                yield return "nrat".FromRawcode();
                yield return "nsea".FromRawcode();
                yield return "nshe".FromRawcode();
                yield return "nskk".FromRawcode();
                yield return "nsno".FromRawcode();
                yield return "nder".FromRawcode();
                yield return "nvul".FromRawcode();
                yield return "nske".FromRawcode();
            }

            // Melee buildings
            {
                // todo
            }

            // Melee heroes
            {
                yield return "Hpal".FromRawcode();
                yield return "Hamg".FromRawcode();
                yield return "Hmkg".FromRawcode();
                yield return "Hblm".FromRawcode();

                yield return "Obla".FromRawcode();
                yield return "Ofar".FromRawcode();
                yield return "Otch".FromRawcode();
                yield return "Oshd".FromRawcode();

                yield return "Udea".FromRawcode();
                yield return "Ulic".FromRawcode();
                yield return "Udre".FromRawcode();
                yield return "Ucrl".FromRawcode();

                yield return "Ekee".FromRawcode();
                yield return "Emoo".FromRawcode();
                yield return "Edem".FromRawcode();
                yield return "Ewar".FromRawcode();

                yield return "Nalc".FromRawcode();
                yield return "Nngs".FromRawcode();
                yield return "Ntin".FromRawcode();
                yield return "Nbst".FromRawcode();
                yield return "Nbrn".FromRawcode();
                yield return "Nfir".FromRawcode();
                yield return "Npbm".FromRawcode();
                yield return "Nplh".FromRawcode();
            }

            // Melee special
            {
                yield return "hrtt".FromRawcode();
                yield return "hmil".FromRawcode();
                yield return "hwat".FromRawcode();
                yield return "hwt2".FromRawcode();
                yield return "hwt3".FromRawcode();
                yield return "hphx".FromRawcode();
                yield return "hpxe".FromRawcode();

                yield return "oeye".FromRawcode();
                yield return "nwad".FromRawcode();
                yield return "otot".FromRawcode();
                yield return "osw1".FromRawcode();
                yield return "osw2".FromRawcode();
                yield return "osw3".FromRawcode();
                yield return "ohwd".FromRawcode();
                yield return "osp1".FromRawcode();
                yield return "osp2".FromRawcode();
                yield return "osp3".FromRawcode();
                yield return "osp4".FromRawcode();
                yield return "ospm".FromRawcode();
                yield return "otbk".FromRawcode();

                yield return "ucrm".FromRawcode();
                yield return "ugrm".FromRawcode();
                yield return "uske".FromRawcode();
                yield return "uskm".FromRawcode();
                yield return "ubsp".FromRawcode();
                yield return "ucs1".FromRawcode();
                yield return "ucsB".FromRawcode();
                yield return "ucs2".FromRawcode();
                yield return "ucsC".FromRawcode();
                yield return "ucs3".FromRawcode();
                yield return "uplg".FromRawcode();
                yield return "uloc".FromRawcode();

                yield return "Edmm".FromRawcode();
                yield return "edtm".FromRawcode();
                yield return "edcm".FromRawcode();
                yield return "efon".FromRawcode();
                yield return "espv".FromRawcode();
                yield return "even".FromRawcode();
                yield return "nowl".FromRawcode();
                yield return "now2".FromRawcode();
                yield return "now3".FromRawcode();
            }

            // Neutral hostile
            {
                // TODO
                // yield return "XXXXXXXXXX".FromRawcode();
                yield return "nslm".FromRawcode();
            }
        }

        public static IEnumerable<int> GetPropertyRawcodes(GamePatch patch)
        {
            if (patch < GamePatch.v1_31_0)
            {
                throw new NotImplementedException();
            }
            else if (patch >= GamePatch.v1_32_0)
            {
                throw new NotImplementedException();

                // Reforged?
                // yield return "uabs".FromRawcode();
                // yield return "uhas".FromRawcode();
            }

            // Abilities
            {
                yield return "udaa".FromRawcode();
                yield return "uabi".FromRawcode();
            }

            // Art
            {
                yield return "utcc".FromRawcode();
                yield return "uble".FromRawcode();
                yield return "ucbs".FromRawcode();
                yield return "ucpt".FromRawcode();
                yield return "urun".FromRawcode();
                yield return "uwal".FromRawcode();
                yield return "ubpx".FromRawcode();
                yield return "ubpy".FromRawcode();
                yield return "ucua".FromRawcode();
                yield return "udtm".FromRawcode();
                yield return "uept".FromRawcode();
                yield return "uerd".FromRawcode();
                yield return "ufrd".FromRawcode();
                yield return "ushr".FromRawcode();
                yield return "uico".FromRawcode();
                yield return "ussi".FromRawcode();
                yield return "umxp".FromRawcode();
                yield return "umxr".FromRawcode();
                yield return "umdl".FromRawcode();
                yield return "uver".FromRawcode();
                yield return "uocc".FromRawcode();
                yield return "uori".FromRawcode();
                yield return "uimz".FromRawcode();
                yield return "uisz".FromRawcode();
                yield return "ulpx".FromRawcode();
                yield return "ulpy".FromRawcode();
                yield return "ulpz".FromRawcode();
                yield return "ulsz".FromRawcode();
                yield return "uprw".FromRawcode();
                yield return "uani".FromRawcode();
                yield return "uaap".FromRawcode();
                yield return "ualp".FromRawcode();
                yield return "ubpr".FromRawcode();
                yield return "uscb".FromRawcode();
                yield return "usca".FromRawcode();
                yield return "uslz".FromRawcode();
                yield return "usew".FromRawcode();
                yield return "ussc".FromRawcode();
                yield return "ushu".FromRawcode();
                yield return "ushx".FromRawcode();
                yield return "ushy".FromRawcode();
                yield return "ushh".FromRawcode();
                yield return "ushw".FromRawcode();
                yield return "ushb".FromRawcode();
                yield return "uspa".FromRawcode();
                yield return "utaa".FromRawcode();
                yield return "utco".FromRawcode();
                yield return "uclr".FromRawcode();
                yield return "uclg".FromRawcode();
                yield return "uclb".FromRawcode();
                yield return "ulos".FromRawcode();
            }

            // Combat
            {
                yield return "uacq".FromRawcode();
                yield return "uarm".FromRawcode();
                yield return "ubs1".FromRawcode();
                yield return "udp1".FromRawcode();
                yield return "ua1f".FromRawcode();
                yield return "ua1h".FromRawcode();
                yield return "ua1q".FromRawcode();
                yield return "ua1p".FromRawcode();
                yield return "ua1t".FromRawcode();
                yield return "ua1c".FromRawcode();
                yield return "ua1b".FromRawcode();
                yield return "uhd1".FromRawcode();
                yield return "uqd1".FromRawcode();
                yield return "udl1".FromRawcode();
                yield return "ua1d".FromRawcode();
                yield return "ua1s".FromRawcode();
                yield return "usd1".FromRawcode();
                yield return "usr1".FromRawcode();
                yield return "udu1".FromRawcode();
                yield return "utc1".FromRawcode();
                yield return "uma1".FromRawcode();
                yield return "ua1m".FromRawcode();
                yield return "umh1".FromRawcode();
                yield return "ua1z".FromRawcode();
                yield return "ua1r".FromRawcode();
                yield return "urb1".FromRawcode();
                yield return "uwu1".FromRawcode();
                yield return "ua1g".FromRawcode();
                yield return "ucs1".FromRawcode();
                yield return "ua1w".FromRawcode();
                yield return "ubs2".FromRawcode();
                yield return "udp2".FromRawcode();
                yield return "ua2f".FromRawcode();
                yield return "ua2h".FromRawcode();
                yield return "ua2q".FromRawcode();
                yield return "ua2p".FromRawcode();
                yield return "ua2t".FromRawcode();
                yield return "ua2c".FromRawcode();
                yield return "ua2b".FromRawcode();
                yield return "uhd2".FromRawcode();
                yield return "uqd2".FromRawcode();
                yield return "udl2".FromRawcode();
                yield return "ua2d".FromRawcode();
                yield return "ua2s".FromRawcode();
                yield return "usd2".FromRawcode();
                yield return "usr2".FromRawcode();
                yield return "udu2".FromRawcode();
                yield return "utc2".FromRawcode();
                yield return "uma2".FromRawcode();
                yield return "ua2m".FromRawcode();
                yield return "umh2".FromRawcode();
                yield return "ua2z".FromRawcode();
                yield return "ua2r".FromRawcode();
                yield return "urb2".FromRawcode();
                yield return "uwu2".FromRawcode();
                yield return "ua2g".FromRawcode();
                yield return "ucs2".FromRawcode();
                yield return "ua2w".FromRawcode();
                yield return "uaen".FromRawcode();
                yield return "udea".FromRawcode();
                yield return "udef".FromRawcode();
                yield return "udty".FromRawcode();
                yield return "udup".FromRawcode();
                yield return "uamn".FromRawcode();
                yield return "utar".FromRawcode();
            }

            // Editor
            {
                yield return "udro".FromRawcode();
                yield return "ucam".FromRawcode();
                yield return "uspe".FromRawcode();
                yield return "uhos".FromRawcode();
                yield return "utss".FromRawcode();
                yield return "uine".FromRawcode();
                yield return "util".FromRawcode();
                yield return "uuch".FromRawcode();
            }

            // Movement
            {
                yield return "urpo".FromRawcode();
                yield return "urpg".FromRawcode();
                yield return "urpp".FromRawcode();
                yield return "urpr".FromRawcode();
                yield return "umvh".FromRawcode();
                yield return "umvf".FromRawcode();
                yield return "umvs".FromRawcode();
                yield return "umas".FromRawcode();
                yield return "umis".FromRawcode();
                yield return "umvr".FromRawcode();
                yield return "umvt".FromRawcode();
            }

            // Pathing
            {
                yield return "uabr".FromRawcode();
                yield return "uabt".FromRawcode();
                yield return "ucol".FromRawcode();
            }

            // Sound
            {
                yield return "ulfi".FromRawcode();
                yield return "ulfo".FromRawcode();
                yield return "umsl".FromRawcode();
                yield return "ursl".FromRawcode();
                yield return "usnd".FromRawcode();
            }

            // Stats
            {
                yield return "ubld".FromRawcode();
                yield return "ufle".FromRawcode();
                yield return "ufoo".FromRawcode();
                yield return "ufma".FromRawcode();
                yield return "ufor".FromRawcode();
                yield return "ubba".FromRawcode();
                yield return "ubdi".FromRawcode();
                yield return "ubsi".FromRawcode();
                yield return "ugol".FromRawcode();
                yield return "uhom".FromRawcode();
                yield return "uhpm".FromRawcode();
                yield return "uhpr".FromRawcode();
                yield return "uhrt".FromRawcode();
                yield return "ubdg".FromRawcode();
                yield return "ulev".FromRawcode();
                yield return "ulba".FromRawcode();
                yield return "ulbd".FromRawcode();
                yield return "ulbs".FromRawcode();
                yield return "ulum".FromRawcode();
                yield return "umpi".FromRawcode();
                yield return "umpm".FromRawcode();
                yield return "umpr".FromRawcode();
                yield return "upoi".FromRawcode();
                yield return "upri".FromRawcode();
                yield return "urac".FromRawcode();
                yield return "ugor".FromRawcode();
                yield return "ulur".FromRawcode();
                yield return "urtm".FromRawcode();
                yield return "usid".FromRawcode();
                yield return "usin".FromRawcode();
                yield return "usle".FromRawcode();
                yield return "usit".FromRawcode();
                yield return "usma".FromRawcode();
                yield return "usrg".FromRawcode();
                yield return "usst".FromRawcode();
                yield return "ucar".FromRawcode();
                yield return "utyp".FromRawcode();
            }

            // Techtree
            {
                yield return "udep".FromRawcode();
                yield return "usei".FromRawcode();
                yield return "ureq".FromRawcode();
                yield return "urqa".FromRawcode();
                yield return "ubui".FromRawcode();
                yield return "useu".FromRawcode();
                yield return "upgr".FromRawcode();
            }

            // Text
            {
                yield return "ucun".FromRawcode();
                yield return "ucut".FromRawcode();
                yield return "ides".FromRawcode();
                yield return "uhot".FromRawcode();
                yield return "unam".FromRawcode();
                yield return "unsf".FromRawcode();
                yield return "utip".FromRawcode();
                yield return "utub".FromRawcode();
            }

            // Hero abilities
            {
                yield return "uhab".FromRawcode();
            }

            // Hero stats
            {
                yield return "uagp".FromRawcode();
                yield return "uhhd".FromRawcode();
                yield return "uhhb".FromRawcode();
                yield return "uhhm".FromRawcode();
                yield return "uinp".FromRawcode();
                yield return "upra".FromRawcode();
                yield return "uagi".FromRawcode();
                yield return "uint".FromRawcode();
                yield return "ustr".FromRawcode();
                yield return "ustp".FromRawcode();
            }

            // Hero techtree
            {
                yield return "urq1".FromRawcode();
                yield return "urq2".FromRawcode();
                yield return "urq3".FromRawcode();
                yield return "urq4".FromRawcode();
                yield return "urq5".FromRawcode();
                yield return "urq6".FromRawcode();
                yield return "urq7".FromRawcode();
                yield return "urq8".FromRawcode();
                yield return "urqc".FromRawcode();
            }

            // Hero text
            {
                yield return "upro".FromRawcode();
                yield return "upru".FromRawcode();
            }

            // Building art
            {
                yield return "uubs".FromRawcode();
            }

            // Building pathing
            {
                yield return "upat".FromRawcode();
                yield return "upar".FromRawcode();
                yield return "upap".FromRawcode();
                yield return "upaw".FromRawcode();
            }

            // Building sound
            {
                yield return "ubsl".FromRawcode();
            }

            // Building stats
            {
                yield return "unbm".FromRawcode();
                yield return "unbr".FromRawcode();
            }

            // Building techtree
            {
                yield return "umki".FromRawcode();
                yield return "ures".FromRawcode();
                yield return "urev".FromRawcode();
                yield return "utra".FromRawcode();
                yield return "uupt".FromRawcode();
            }
        }

        private static int FromRawcode(this string code)
        {
            if ((code?.Length ?? 0) != 4)
            {
                return 0;
            }

            return (code[0]) | (code[1] << 8) | (code[2] << 16) | (code[3] << 24);
        }

    }
}