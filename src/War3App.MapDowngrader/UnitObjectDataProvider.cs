﻿using System;
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
            switch (patch)
            {
                case GamePatch.v1_29_0:
                case GamePatch.v1_29_1:
                case GamePatch.v1_29_2:
                    return GetRawcodesPatch1_29_2();

                case GamePatch.v1_31_0:
                case GamePatch.v1_31_1:
                    return GetRawcodesPatch1_31_1();

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
                    return GetPropertyRawcodesPatch1_31_1();

                default:
                    throw new NotImplementedException();
            }

            // Reforged?
            // yield return "uabs".FromRawcode();
            // yield return "uhas".FromRawcode();
        }

        private static IEnumerable<int> GetRawcodesPatch1_29_2()
        {
            yield return "Hamg".FromRawcode();
            yield return "Hblm".FromRawcode();
            yield return "Hmkg".FromRawcode();
            yield return "Hpal".FromRawcode();
            yield return "hbot".FromRawcode();
            yield return "hbsh".FromRawcode();
            yield return "hdes".FromRawcode();
            yield return "hdhw".FromRawcode();
            yield return "hfoo".FromRawcode();
            yield return "hgry".FromRawcode();
            yield return "hgyr".FromRawcode();
            yield return "hkni".FromRawcode();
            yield return "hmil".FromRawcode();
            yield return "hmpr".FromRawcode();
            yield return "hmtm".FromRawcode();
            yield return "hmtt".FromRawcode();
            yield return "hpea".FromRawcode();
            yield return "hphx".FromRawcode();
            yield return "hpxe".FromRawcode();
            yield return "hrif".FromRawcode();
            yield return "hrtt".FromRawcode();
            yield return "hsor".FromRawcode();
            yield return "hspt".FromRawcode();
            yield return "hwat".FromRawcode();
            yield return "hwt2".FromRawcode();
            yield return "hwt3".FromRawcode();
            yield return "nlv1".FromRawcode();
            yield return "nlv2".FromRawcode();
            yield return "nlv3".FromRawcode();
            yield return "halt".FromRawcode();
            yield return "harm".FromRawcode();
            yield return "hars".FromRawcode();
            yield return "hatw".FromRawcode();
            yield return "hbar".FromRawcode();
            yield return "hbla".FromRawcode();
            yield return "hcas".FromRawcode();
            yield return "hctw".FromRawcode();
            yield return "hgra".FromRawcode();
            yield return "hgtw".FromRawcode();
            yield return "hhou".FromRawcode();
            yield return "hkee".FromRawcode();
            yield return "hlum".FromRawcode();
            yield return "hshy".FromRawcode();
            yield return "htow".FromRawcode();
            yield return "hvlt".FromRawcode();
            yield return "hwtw".FromRawcode();
            yield return "Obla".FromRawcode();
            yield return "Ofar".FromRawcode();
            yield return "Oshd".FromRawcode();
            yield return "Otch".FromRawcode();
            yield return "ncat".FromRawcode();
            yield return "nsw1".FromRawcode();
            yield return "nsw2".FromRawcode();
            yield return "nsw3".FromRawcode();
            yield return "nwad".FromRawcode();
            yield return "obot".FromRawcode();
            yield return "ocat".FromRawcode();
            yield return "odes".FromRawcode();
            yield return "odoc".FromRawcode();
            yield return "oeye".FromRawcode();
            yield return "ogru".FromRawcode();
            yield return "ohun".FromRawcode();
            yield return "ohwd".FromRawcode();
            yield return "okod".FromRawcode();
            yield return "opeo".FromRawcode();
            yield return "orai".FromRawcode();
            yield return "oshm".FromRawcode();
            yield return "osp1".FromRawcode();
            yield return "osp2".FromRawcode();
            yield return "osp3".FromRawcode();
            yield return "osp4".FromRawcode();
            yield return "ospm".FromRawcode();
            yield return "ospw".FromRawcode();
            yield return "osw1".FromRawcode();
            yield return "osw2".FromRawcode();
            yield return "osw3".FromRawcode();
            yield return "otau".FromRawcode();
            yield return "otbk".FromRawcode();
            yield return "otbr".FromRawcode();
            yield return "otot".FromRawcode();
            yield return "owyv".FromRawcode();
            yield return "oalt".FromRawcode();
            yield return "obar".FromRawcode();
            yield return "obea".FromRawcode();
            yield return "ofor".FromRawcode();
            yield return "ofrt".FromRawcode();
            yield return "ogre".FromRawcode();
            yield return "oshy".FromRawcode();
            yield return "osld".FromRawcode();
            yield return "ostr".FromRawcode();
            yield return "otrb".FromRawcode();
            yield return "otto".FromRawcode();
            yield return "ovln".FromRawcode();
            yield return "owtw".FromRawcode();
            yield return "Edem".FromRawcode();
            yield return "Edmm".FromRawcode();
            yield return "Ekee".FromRawcode();
            yield return "Emoo".FromRawcode();
            yield return "Ewar".FromRawcode();
            yield return "earc".FromRawcode();
            yield return "ebal".FromRawcode();
            yield return "ebsh".FromRawcode();
            yield return "echm".FromRawcode();
            yield return "edcm".FromRawcode();
            yield return "edes".FromRawcode();
            yield return "edoc".FromRawcode();
            yield return "edot".FromRawcode();
            yield return "edry".FromRawcode();
            yield return "edtm".FromRawcode();
            yield return "efdr".FromRawcode();
            yield return "efon".FromRawcode();
            yield return "ehip".FromRawcode();
            yield return "ehpr".FromRawcode();
            yield return "emtg".FromRawcode();
            yield return "esen".FromRawcode();
            yield return "espv".FromRawcode();
            yield return "even".FromRawcode();
            yield return "ewsp".FromRawcode();
            yield return "eaoe".FromRawcode();
            yield return "eaom".FromRawcode();
            yield return "eaow".FromRawcode();
            yield return "eate".FromRawcode();
            yield return "eden".FromRawcode();
            yield return "edob".FromRawcode();
            yield return "edos".FromRawcode();
            yield return "egol".FromRawcode();
            yield return "emow".FromRawcode();
            yield return "eshy".FromRawcode();
            yield return "etoa".FromRawcode();
            yield return "etoe".FromRawcode();
            yield return "etol".FromRawcode();
            yield return "etrp".FromRawcode();
            yield return "Ucrl".FromRawcode();
            yield return "Udea".FromRawcode();
            yield return "Udre".FromRawcode();
            yield return "Ulic".FromRawcode();
            yield return "uabo".FromRawcode();
            yield return "uaco".FromRawcode();
            yield return "uban".FromRawcode();
            yield return "ubsp".FromRawcode();
            yield return "ucrm".FromRawcode();
            yield return "ucry".FromRawcode();
            yield return "ucs1".FromRawcode();
            yield return "ucs2".FromRawcode();
            yield return "ucs3".FromRawcode();
            yield return "ucsB".FromRawcode();
            yield return "ucsC".FromRawcode();
            yield return "ufro".FromRawcode();
            yield return "ugar".FromRawcode();
            yield return "ugho".FromRawcode();
            yield return "ugrm".FromRawcode();
            yield return "uloc".FromRawcode();
            yield return "umtw".FromRawcode();
            yield return "unec".FromRawcode();
            yield return "uobs".FromRawcode();
            yield return "uplg".FromRawcode();
            yield return "ushd".FromRawcode();
            yield return "uske".FromRawcode();
            yield return "uskm".FromRawcode();
            yield return "uubs".FromRawcode();
            yield return "uaod".FromRawcode();
            yield return "ubon".FromRawcode();
            yield return "ugol".FromRawcode();
            yield return "ugrv".FromRawcode();
            yield return "unp1".FromRawcode();
            yield return "unp2".FromRawcode();
            yield return "unpl".FromRawcode();
            yield return "usap".FromRawcode();
            yield return "usep".FromRawcode();
            yield return "ushp".FromRawcode();
            yield return "uslh".FromRawcode();
            yield return "utod".FromRawcode();
            yield return "utom".FromRawcode();
            yield return "uzg1".FromRawcode();
            yield return "uzg2".FromRawcode();
            yield return "uzig".FromRawcode();
            yield return "Nal2".FromRawcode();
            yield return "Nal3".FromRawcode();
            yield return "Nalc".FromRawcode();
            yield return "Nalm".FromRawcode();
            yield return "Nbrn".FromRawcode();
            yield return "Nbst".FromRawcode();
            yield return "Nfir".FromRawcode();
            yield return "Nngs".FromRawcode();
            yield return "Npbm".FromRawcode();
            yield return "Nplh".FromRawcode();
            yield return "Nrob".FromRawcode();
            yield return "Ntin".FromRawcode();
            yield return "ncg1".FromRawcode();
            yield return "ncg2".FromRawcode();
            yield return "ncg3".FromRawcode();
            yield return "ncgb".FromRawcode();
            yield return "ndr1".FromRawcode();
            yield return "ndr2".FromRawcode();
            yield return "ndr3".FromRawcode();
            yield return "nfa1".FromRawcode();
            yield return "nfa2".FromRawcode();
            yield return "nfac".FromRawcode();
            yield return "ngz1".FromRawcode();
            yield return "ngz2".FromRawcode();
            yield return "ngz3".FromRawcode();
            yield return "ngz4".FromRawcode();
            yield return "ngzc".FromRawcode();
            yield return "ngzd".FromRawcode();
            yield return "npn1".FromRawcode();
            yield return "npn2".FromRawcode();
            yield return "npn3".FromRawcode();
            yield return "npn4".FromRawcode();
            yield return "npn5".FromRawcode();
            yield return "npn6".FromRawcode();
            yield return "nqb1".FromRawcode();
            yield return "nqb2".FromRawcode();
            yield return "nqb3".FromRawcode();
            yield return "nqb4".FromRawcode();
            yield return "nwe1".FromRawcode();
            yield return "nwe2".FromRawcode();
            yield return "nwe3".FromRawcode();
            yield return "nadk".FromRawcode();
            yield return "nadr".FromRawcode();
            yield return "nadw".FromRawcode();
            yield return "nahy".FromRawcode();
            yield return "nanb".FromRawcode();
            yield return "nanc".FromRawcode();
            yield return "nane".FromRawcode();
            yield return "nanm".FromRawcode();
            yield return "nano".FromRawcode();
            yield return "nanw".FromRawcode();
            yield return "narg".FromRawcode();
            yield return "nass".FromRawcode();
            yield return "nba2".FromRawcode();
            yield return "nbal".FromRawcode();
            yield return "nban".FromRawcode();
            yield return "nbda".FromRawcode();
            yield return "nbdk".FromRawcode();
            yield return "nbdm".FromRawcode();
            yield return "nbdo".FromRawcode();
            yield return "nbdr".FromRawcode();
            yield return "nbds".FromRawcode();
            yield return "nbdw".FromRawcode();
            yield return "nbld".FromRawcode();
            yield return "nbnb".FromRawcode();
            yield return "nbot".FromRawcode();
            yield return "nbrg".FromRawcode();
            yield return "nbwm".FromRawcode();
            yield return "nbzd".FromRawcode();
            yield return "nbzk".FromRawcode();
            yield return "nbzw".FromRawcode();
            yield return "ncea".FromRawcode();
            yield return "ncen".FromRawcode();
            yield return "ncer".FromRawcode();
            yield return "ncfs".FromRawcode();
            yield return "nchp".FromRawcode();
            yield return "ncim".FromRawcode();
            yield return "ncks".FromRawcode();
            yield return "ncnk".FromRawcode();
            yield return "ndqn".FromRawcode();
            yield return "ndqp".FromRawcode();
            yield return "ndqs".FromRawcode();
            yield return "ndqt".FromRawcode();
            yield return "ndqv".FromRawcode();
            yield return "ndrv".FromRawcode();
            yield return "ndtb".FromRawcode();
            yield return "ndth".FromRawcode();
            yield return "ndtp".FromRawcode();
            yield return "ndtr".FromRawcode();
            yield return "ndtt".FromRawcode();
            yield return "ndtw".FromRawcode();
            yield return "nehy".FromRawcode();
            yield return "nelb".FromRawcode();
            yield return "nele".FromRawcode();
            yield return "nenc".FromRawcode();
            yield return "nenf".FromRawcode();
            yield return "nenp".FromRawcode();
            yield return "nepl".FromRawcode();
            yield return "nerd".FromRawcode();
            yield return "ners".FromRawcode();
            yield return "nerw".FromRawcode();
            yield return "nfel".FromRawcode();
            yield return "nfgb".FromRawcode();
            yield return "nfgo".FromRawcode();
            yield return "nfgt".FromRawcode();
            yield return "nfgu".FromRawcode();
            yield return "nfod".FromRawcode();
            yield return "nfor".FromRawcode();
            yield return "nfot".FromRawcode();
            yield return "nfov".FromRawcode();
            yield return "nfpc".FromRawcode();
            yield return "nfpe".FromRawcode();
            yield return "nfpl".FromRawcode();
            yield return "nfps".FromRawcode();
            yield return "nfpt".FromRawcode();
            yield return "nfpu".FromRawcode();
            yield return "nfra".FromRawcode();
            yield return "nfrb".FromRawcode();
            yield return "nfre".FromRawcode();
            yield return "nfrg".FromRawcode();
            yield return "nfrl".FromRawcode();
            yield return "nfrp".FromRawcode();
            yield return "nfrs".FromRawcode();
            yield return "nfsh".FromRawcode();
            yield return "nfsp".FromRawcode();
            yield return "nftb".FromRawcode();
            yield return "nftk".FromRawcode();
            yield return "nftr".FromRawcode();
            yield return "nftt".FromRawcode();
            yield return "ngdk".FromRawcode();
            yield return "nggr".FromRawcode();
            yield return "ngh1".FromRawcode();
            yield return "ngh2".FromRawcode();
            yield return "ngir".FromRawcode();
            yield return "nglm".FromRawcode();
            yield return "ngna".FromRawcode();
            yield return "ngnb".FromRawcode();
            yield return "ngno".FromRawcode();
            yield return "ngns".FromRawcode();
            yield return "ngnv".FromRawcode();
            yield return "ngnw".FromRawcode();
            yield return "ngrd".FromRawcode();
            yield return "ngrk".FromRawcode();
            yield return "ngrw".FromRawcode();
            yield return "ngsp".FromRawcode();
            yield return "ngst".FromRawcode();
            yield return "ngza".FromRawcode();
            yield return "nhar".FromRawcode();
            yield return "nhdc".FromRawcode();
            yield return "nhfp".FromRawcode();
            yield return "nhhr".FromRawcode();
            yield return "nhrh".FromRawcode();
            yield return "nhrq".FromRawcode();
            yield return "nhrr".FromRawcode();
            yield return "nhrw".FromRawcode();
            yield return "nhyc".FromRawcode();
            yield return "nhyd".FromRawcode();
            yield return "nhyh".FromRawcode();
            yield return "nhym".FromRawcode();
            yield return "nina".FromRawcode();
            yield return "ninc".FromRawcode();
            yield return "ninf".FromRawcode();
            yield return "ninm".FromRawcode();
            yield return "nith".FromRawcode();
            yield return "nitp".FromRawcode();
            yield return "nitr".FromRawcode();
            yield return "nits".FromRawcode();
            yield return "nitt".FromRawcode();
            yield return "nitw".FromRawcode();
            yield return "njg1".FromRawcode();
            yield return "njga".FromRawcode();
            yield return "njgb".FromRawcode();
            yield return "nkob".FromRawcode();
            yield return "nkog".FromRawcode();
            yield return "nkol".FromRawcode();
            yield return "nkot".FromRawcode();
            yield return "nlds".FromRawcode();
            yield return "nlkl".FromRawcode();
            yield return "nlpd".FromRawcode();
            yield return "nlpr".FromRawcode();
            yield return "nlps".FromRawcode();
            yield return "nlrv".FromRawcode();
            yield return "nlsn".FromRawcode();
            yield return "nltc".FromRawcode();
            yield return "nltl".FromRawcode();
            yield return "nlur".FromRawcode();
            yield return "nmam".FromRawcode();
            yield return "nmbg".FromRawcode();
            yield return "nmcf".FromRawcode();
            yield return "nmdr".FromRawcode();
            yield return "nmfs".FromRawcode();
            yield return "nmgd".FromRawcode();
            yield return "nmgr".FromRawcode();
            yield return "nmgw".FromRawcode();
            yield return "nmit".FromRawcode();
            yield return "nmmu".FromRawcode();
            yield return "nmpg".FromRawcode();
            yield return "nmrl".FromRawcode();
            yield return "nmrm".FromRawcode();
            yield return "nmrr".FromRawcode();
            yield return "nmrv".FromRawcode();
            yield return "nmsc".FromRawcode();
            yield return "nmsn".FromRawcode();
            yield return "nmtw".FromRawcode();
            yield return "nmyr".FromRawcode();
            yield return "nmys".FromRawcode();
            yield return "nndk".FromRawcode();
            yield return "nndr".FromRawcode();
            yield return "nnht".FromRawcode();
            yield return "nnmg".FromRawcode();
            yield return "nnrg".FromRawcode();
            yield return "nnrs".FromRawcode();
            yield return "nnsu".FromRawcode();
            yield return "nnsw".FromRawcode();
            yield return "nnwa".FromRawcode();
            yield return "nnwl".FromRawcode();
            yield return "nnwq".FromRawcode();
            yield return "nnwr".FromRawcode();
            yield return "nnws".FromRawcode();
            yield return "noga".FromRawcode();
            yield return "nogl".FromRawcode();
            yield return "nogm".FromRawcode();
            yield return "nogn".FromRawcode();
            yield return "nogo".FromRawcode();
            yield return "nogr".FromRawcode();
            yield return "nomg".FromRawcode();
            yield return "nowb".FromRawcode();
            yield return "nowe".FromRawcode();
            yield return "nowk".FromRawcode();
            yield return "npfl".FromRawcode();
            yield return "npfm".FromRawcode();
            yield return "nplb".FromRawcode();
            yield return "nplg".FromRawcode();
            yield return "nqbh".FromRawcode();
            yield return "nrdk".FromRawcode();
            yield return "nrdr".FromRawcode();
            yield return "nrel".FromRawcode();
            yield return "nrog".FromRawcode();
            yield return "nrvd".FromRawcode();
            yield return "nrvf".FromRawcode();
            yield return "nrvi".FromRawcode();
            yield return "nrvl".FromRawcode();
            yield return "nrvs".FromRawcode();
            yield return "nrwm".FromRawcode();
            yield return "nrzb".FromRawcode();
            yield return "nrzg".FromRawcode();
            yield return "nrzm".FromRawcode();
            yield return "nrzs".FromRawcode();
            yield return "nrzt".FromRawcode();
            yield return "nsat".FromRawcode();
            yield return "nsbm".FromRawcode();
            yield return "nsbs".FromRawcode();
            yield return "nsc2".FromRawcode();
            yield return "nsc3".FromRawcode();
            yield return "nsca".FromRawcode();
            yield return "nscb".FromRawcode();
            yield return "nsce".FromRawcode();
            yield return "nsel".FromRawcode();
            yield return "nsgb".FromRawcode();
            yield return "nsgg".FromRawcode();
            yield return "nsgh".FromRawcode();
            yield return "nsgn".FromRawcode();
            yield return "nsgt".FromRawcode();
            yield return "nska".FromRawcode();
            yield return "nske".FromRawcode();
            yield return "nskf".FromRawcode();
            yield return "nskg".FromRawcode();
            yield return "nskm".FromRawcode();
            yield return "nsko".FromRawcode();
            yield return "nslf".FromRawcode();
            yield return "nslh".FromRawcode();
            yield return "nsll".FromRawcode();
            yield return "nslm".FromRawcode();
            yield return "nsln".FromRawcode();
            yield return "nslr".FromRawcode();
            yield return "nslv".FromRawcode();
            yield return "nsnp".FromRawcode();
            yield return "nsns".FromRawcode();
            yield return "nsoc".FromRawcode();
            yield return "nsog".FromRawcode();
            yield return "nspb".FromRawcode();
            yield return "nspd".FromRawcode();
            yield return "nspg".FromRawcode();
            yield return "nspp".FromRawcode();
            yield return "nspr".FromRawcode();
            yield return "nsqa".FromRawcode();
            yield return "nsqe".FromRawcode();
            yield return "nsqo".FromRawcode();
            yield return "nsqt".FromRawcode();
            yield return "nsra".FromRawcode();
            yield return "nsrh".FromRawcode();
            yield return "nsrn".FromRawcode();
            yield return "nsrv".FromRawcode();
            yield return "nsrw".FromRawcode();
            yield return "nssp".FromRawcode();
            yield return "nsth".FromRawcode();
            yield return "nstl".FromRawcode();
            yield return "nsts".FromRawcode();
            yield return "nstw".FromRawcode();
            yield return "nsty".FromRawcode();
            yield return "nthl".FromRawcode();
            yield return "ntka".FromRawcode();
            yield return "ntkc".FromRawcode();
            yield return "ntkf".FromRawcode();
            yield return "ntkh".FromRawcode();
            yield return "ntks".FromRawcode();
            yield return "ntkt".FromRawcode();
            yield return "ntkw".FromRawcode();
            yield return "ntor".FromRawcode();
            yield return "ntrd".FromRawcode();
            yield return "ntrg".FromRawcode();
            yield return "ntrh".FromRawcode();
            yield return "ntrs".FromRawcode();
            yield return "ntrt".FromRawcode();
            yield return "ntrv".FromRawcode();
            yield return "ntws".FromRawcode();
            yield return "nubk".FromRawcode();
            yield return "nubr".FromRawcode();
            yield return "nubw".FromRawcode();
            yield return "nvde".FromRawcode();
            yield return "nvdg".FromRawcode();
            yield return "nvdl".FromRawcode();
            yield return "nvdw".FromRawcode();
            yield return "nwen".FromRawcode();
            yield return "nwgs".FromRawcode();
            yield return "nwiz".FromRawcode();
            yield return "nwld".FromRawcode();
            yield return "nwlg".FromRawcode();
            yield return "nwlt".FromRawcode();
            yield return "nwna".FromRawcode();
            yield return "nwnr".FromRawcode();
            yield return "nwns".FromRawcode();
            yield return "nwrg".FromRawcode();
            yield return "nws1".FromRawcode();
            yield return "nwwd".FromRawcode();
            yield return "nwwf".FromRawcode();
            yield return "nwwg".FromRawcode();
            yield return "nwzd".FromRawcode();
            yield return "nwzg".FromRawcode();
            yield return "nwzr".FromRawcode();
            yield return "nzep".FromRawcode();
            yield return "nzom".FromRawcode();
            yield return "nalb".FromRawcode();
            yield return "ncrb".FromRawcode();
            yield return "nder".FromRawcode();
            yield return "ndog".FromRawcode();
            yield return "ndwm".FromRawcode();
            yield return "nech".FromRawcode();
            yield return "necr".FromRawcode();
            yield return "nfbr".FromRawcode();
            yield return "nfro".FromRawcode();
            yield return "nhmc".FromRawcode();
            yield return "now2".FromRawcode();
            yield return "now3".FromRawcode();
            yield return "nowl".FromRawcode();
            yield return "npig".FromRawcode();
            yield return "npng".FromRawcode();
            yield return "npnw".FromRawcode();
            yield return "nrac".FromRawcode();
            yield return "nrat".FromRawcode();
            yield return "nsea".FromRawcode();
            yield return "nsha".FromRawcode();
            yield return "nshe".FromRawcode();
            yield return "nshf".FromRawcode();
            yield return "nshw".FromRawcode();
            yield return "nskk".FromRawcode();
            yield return "nsno".FromRawcode();
            yield return "nvil".FromRawcode();
            yield return "nvk2".FromRawcode();
            yield return "nvl2".FromRawcode();
            yield return "nvlk".FromRawcode();
            yield return "nvlw".FromRawcode();
            yield return "nvul".FromRawcode();
            yield return "ncb0".FromRawcode();
            yield return "ncb1".FromRawcode();
            yield return "ncb2".FromRawcode();
            yield return "ncb3".FromRawcode();
            yield return "ncb4".FromRawcode();
            yield return "ncb5".FromRawcode();
            yield return "ncb6".FromRawcode();
            yield return "ncb7".FromRawcode();
            yield return "ncb8".FromRawcode();
            yield return "ncb9".FromRawcode();
            yield return "ncba".FromRawcode();
            yield return "ncbb".FromRawcode();
            yield return "ncbc".FromRawcode();
            yield return "ncbd".FromRawcode();
            yield return "ncbe".FromRawcode();
            yield return "ncbf".FromRawcode();
            yield return "ncnt".FromRawcode();
            yield return "ncop".FromRawcode();
            yield return "ncp2".FromRawcode();
            yield return "ncp3".FromRawcode();
            yield return "nct1".FromRawcode();
            yield return "nct2".FromRawcode();
            yield return "ndch".FromRawcode();
            yield return "ndh0".FromRawcode();
            yield return "ndh1".FromRawcode();
            yield return "ndh2".FromRawcode();
            yield return "ndh3".FromRawcode();
            yield return "ndh4".FromRawcode();
            yield return "ndrg".FromRawcode();
            yield return "ndrk".FromRawcode();
            yield return "ndro".FromRawcode();
            yield return "ndrr".FromRawcode();
            yield return "ndru".FromRawcode();
            yield return "ndrz".FromRawcode();
            yield return "nfh0".FromRawcode();
            yield return "nfh1".FromRawcode();
            yield return "nfoh".FromRawcode();
            yield return "nfr1".FromRawcode();
            yield return "nfr2".FromRawcode();
            yield return "ngad".FromRawcode();
            yield return "ngme".FromRawcode();
            yield return "ngnh".FromRawcode();
            yield return "ngni".FromRawcode();
            yield return "ngol".FromRawcode();
            yield return "ngt2".FromRawcode();
            yield return "ngwr".FromRawcode();
            yield return "nhns".FromRawcode();
            yield return "nmer".FromRawcode();
            yield return "nmg0".FromRawcode();
            yield return "nmg1".FromRawcode();
            yield return "nmh0".FromRawcode();
            yield return "nmh1".FromRawcode();
            yield return "nmoo".FromRawcode();
            yield return "nmr0".FromRawcode();
            yield return "nmr2".FromRawcode();
            yield return "nmr3".FromRawcode();
            yield return "nmr4".FromRawcode();
            yield return "nmr5".FromRawcode();
            yield return "nmr6".FromRawcode();
            yield return "nmr7".FromRawcode();
            yield return "nmr8".FromRawcode();
            yield return "nmr9".FromRawcode();
            yield return "nmra".FromRawcode();
            yield return "nmrb".FromRawcode();
            yield return "nmrc".FromRawcode();
            yield return "nmrd".FromRawcode();
            yield return "nmre".FromRawcode();
            yield return "nmrf".FromRawcode();
            yield return "nmrk".FromRawcode();
            yield return "nnzg".FromRawcode();
            yield return "nshp".FromRawcode();
            yield return "ntav".FromRawcode();
            yield return "nten".FromRawcode();
            yield return "nth0".FromRawcode();
            yield return "nth1".FromRawcode();
            yield return "ntn2".FromRawcode();
            yield return "ntnt".FromRawcode();
            yield return "ntt2".FromRawcode();
            yield return "nwgt".FromRawcode();
            yield return "Ecen".FromRawcode();
            yield return "Eevi".FromRawcode();
            yield return "Eevm".FromRawcode();
            yield return "Efur".FromRawcode();
            yield return "Eidm".FromRawcode();
            yield return "Eill".FromRawcode();
            yield return "Eilm".FromRawcode();
            yield return "Ekgg".FromRawcode();
            yield return "Emfr".FromRawcode();
            yield return "Emns".FromRawcode();
            yield return "Etyr".FromRawcode();
            yield return "Ewrd".FromRawcode();
            yield return "Hant".FromRawcode();
            yield return "Hapm".FromRawcode();
            yield return "Harf".FromRawcode();
            yield return "Hart".FromRawcode();
            yield return "Hdgo".FromRawcode();
            yield return "Hgam".FromRawcode();
            yield return "Hhkl".FromRawcode();
            yield return "Hjai".FromRawcode();
            yield return "Hkal".FromRawcode();
            yield return "Hlgr".FromRawcode();
            yield return "Hmbr".FromRawcode();
            yield return "Hmgd".FromRawcode();
            yield return "Hpb1".FromRawcode();
            yield return "Hpb2".FromRawcode();
            yield return "Huth".FromRawcode();
            yield return "Hvsh".FromRawcode();
            yield return "Hvwd".FromRawcode();
            yield return "Naka".FromRawcode();
            yield return "Nbbc".FromRawcode();
            yield return "Nkjx".FromRawcode();
            yield return "Nklj".FromRawcode();
            yield return "Nmag".FromRawcode();
            yield return "Nman".FromRawcode();
            yield return "Npld".FromRawcode();
            yield return "Nsjs".FromRawcode();
            yield return "Ocb2".FromRawcode();
            yield return "Ocbh".FromRawcode();
            yield return "Odrt".FromRawcode();
            yield return "Ogld".FromRawcode();
            yield return "Ogrh".FromRawcode();
            yield return "Opgh".FromRawcode();
            yield return "Orex".FromRawcode();
            yield return "Orkn".FromRawcode();
            yield return "Osam".FromRawcode();
            yield return "Otcc".FromRawcode();
            yield return "Othr".FromRawcode();
            yield return "Uanb".FromRawcode();
            yield return "Ubal".FromRawcode();
            yield return "Uclc".FromRawcode();
            yield return "Udth".FromRawcode();
            yield return "Uear".FromRawcode();
            yield return "Uktl".FromRawcode();
            yield return "Umal".FromRawcode();
            yield return "Usyl".FromRawcode();
            yield return "Utic".FromRawcode();
            yield return "Uvar".FromRawcode();
            yield return "Uvng".FromRawcode();
            yield return "Uwar".FromRawcode();
            yield return "eilw".FromRawcode();
            yield return "enec".FromRawcode();
            yield return "ensh".FromRawcode();
            yield return "eshd".FromRawcode();
            yield return "etrs".FromRawcode();
            yield return "hbew".FromRawcode();
            yield return "hcth".FromRawcode();
            yield return "hhdl".FromRawcode();
            yield return "hhes".FromRawcode();
            yield return "hprt".FromRawcode();
            yield return "hrdh".FromRawcode();
            yield return "nbee".FromRawcode();
            yield return "nbel".FromRawcode();
            yield return "nbsp".FromRawcode();
            yield return "nchg".FromRawcode();
            yield return "nchr".FromRawcode();
            yield return "nchw".FromRawcode();
            yield return "nckb".FromRawcode();
            yield return "ncpn".FromRawcode();
            yield return "ndmu".FromRawcode();
            yield return "ndrd".FromRawcode();
            yield return "ndrf".FromRawcode();
            yield return "ndrh".FromRawcode();
            yield return "ndrj".FromRawcode();
            yield return "ndrl".FromRawcode();
            yield return "ndrm".FromRawcode();
            yield return "ndrn".FromRawcode();
            yield return "ndrp".FromRawcode();
            yield return "ndrs".FromRawcode();
            yield return "ndrt".FromRawcode();
            yield return "ndrw".FromRawcode();
            yield return "ndsa".FromRawcode();
            yield return "negz".FromRawcode();
            yield return "nemi".FromRawcode();
            yield return "nfgl".FromRawcode();
            yield return "ngbl".FromRawcode();
            yield return "nhea".FromRawcode();
            yield return "nhef".FromRawcode();
            yield return "nhem".FromRawcode();
            yield return "nhew".FromRawcode();
            yield return "njks".FromRawcode();
            yield return "nmdm".FromRawcode();
            yield return "nmed".FromRawcode();
            yield return "nmpe".FromRawcode();
            yield return "nmsh".FromRawcode();
            yield return "nser".FromRawcode();
            yield return "nspc".FromRawcode();
            yield return "nssn".FromRawcode();
            yield return "nthr".FromRawcode();
            yield return "nw2w".FromRawcode();
            yield return "nwat".FromRawcode();
            yield return "nzlc".FromRawcode();
            yield return "odkt".FromRawcode();
            yield return "ogrk".FromRawcode();
            yield return "ojgn".FromRawcode();
            yield return "omtg".FromRawcode();
            yield return "onzg".FromRawcode();
            yield return "oosc".FromRawcode();
            yield return "oswy".FromRawcode();
            yield return "ovlj".FromRawcode();
            yield return "owar".FromRawcode();
            yield return "ownr".FromRawcode();
            yield return "uabc".FromRawcode();
            yield return "uarb".FromRawcode();
            yield return "ubdd".FromRawcode();
            yield return "ubdr".FromRawcode();
            yield return "ubot".FromRawcode();
            yield return "udes".FromRawcode();
            yield return "uktg".FromRawcode();
            yield return "uktn".FromRawcode();
            yield return "uswb".FromRawcode();
            yield return "haro".FromRawcode();
            yield return "nbfl".FromRawcode();
            yield return "nbse".FromRawcode();
            yield return "nbsm".FromRawcode();
            yield return "nbsw".FromRawcode();
            yield return "nbt1".FromRawcode();
            yield return "nbt2".FromRawcode();
            yield return "nbwd".FromRawcode();
            yield return "ncap".FromRawcode();
            yield return "ncaw".FromRawcode();
            yield return "ncmw".FromRawcode();
            yield return "ncta".FromRawcode();
            yield return "ncte".FromRawcode();
            yield return "nctl".FromRawcode();
            yield return "ndfl".FromRawcode();
            yield return "ndgt".FromRawcode();
            yield return "ndke".FromRawcode();
            yield return "ndkw".FromRawcode();
            yield return "ndmg".FromRawcode();
            yield return "ndrb".FromRawcode();
            yield return "ndt1".FromRawcode();
            yield return "ndt2".FromRawcode();
            yield return "nef0".FromRawcode();
            yield return "nef1".FromRawcode();
            yield return "nef2".FromRawcode();
            yield return "nef3".FromRawcode();
            yield return "nef4".FromRawcode();
            yield return "nef5".FromRawcode();
            yield return "nef6".FromRawcode();
            yield return "nef7".FromRawcode();
            yield return "nefm".FromRawcode();
            yield return "negf".FromRawcode();
            yield return "negm".FromRawcode();
            yield return "negt".FromRawcode();
            yield return "net1".FromRawcode();
            yield return "net2".FromRawcode();
            yield return "nfnp".FromRawcode();
            yield return "nfrm".FromRawcode();
            yield return "nfrt".FromRawcode();
            yield return "nft1".FromRawcode();
            yield return "nft2".FromRawcode();
            yield return "nfv0".FromRawcode();
            yield return "nfv1".FromRawcode();
            yield return "nfv2".FromRawcode();
            yield return "nfv3".FromRawcode();
            yield return "nfv4".FromRawcode();
            yield return "ngob".FromRawcode();
            yield return "nhcn".FromRawcode();
            yield return "nheb".FromRawcode();
            yield return "nico".FromRawcode();
            yield return "nitb".FromRawcode();
            yield return "nmgv".FromRawcode();
            yield return "nnad".FromRawcode();
            yield return "nnfm".FromRawcode();
            yield return "nnsa".FromRawcode();
            yield return "nnsg".FromRawcode();
            yield return "nntg".FromRawcode();
            yield return "nntt".FromRawcode();
            yield return "npgf".FromRawcode();
            yield return "npgr".FromRawcode();
            yield return "nshr".FromRawcode();
            yield return "ntt1".FromRawcode();
            yield return "ntx2".FromRawcode();
            yield return "nvr0".FromRawcode();
            yield return "nvr1".FromRawcode();
            yield return "nvr2".FromRawcode();
            yield return "nwc1".FromRawcode();
            yield return "nwc2".FromRawcode();
            yield return "nzin".FromRawcode();
            yield return "ocbw".FromRawcode();
            yield return "zcso".FromRawcode();
            yield return "zhyd".FromRawcode();
            yield return "zjug".FromRawcode();
            yield return "zmar".FromRawcode();
            yield return "zshv".FromRawcode();
            yield return "zsmc".FromRawcode();
            yield return "zzrg".FromRawcode();
        }

        private static IEnumerable<int> GetRawcodesPatch1_31_1()
        {
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

        private static IEnumerable<int> GetPropertyRawcodesPatch1_29_2()
        {
            yield return "uaap".FromRawcode();
            yield return "ualp".FromRawcode();
            yield return "uawt".FromRawcode();
            yield return "ubpr".FromRawcode();
            yield return "ubsl".FromRawcode();
            yield return "ubui".FromRawcode();
            yield return "ubpx".FromRawcode();
            yield return "ubpy".FromRawcode();
            yield return "ucua".FromRawcode();
            yield return "ucun".FromRawcode();
            yield return "ucut".FromRawcode();
            yield return "udep".FromRawcode();
            yield return "ides".FromRawcode();
            yield return "unsf".FromRawcode();
            yield return "uhot".FromRawcode();
            yield return "ulfi".FromRawcode();
            yield return "ulfo".FromRawcode();
            yield return "umki".FromRawcode();
            yield return "uma1".FromRawcode();
            yield return "uma2".FromRawcode();
            yield return "ua1m".FromRawcode();
            yield return "ua2m".FromRawcode();
            yield return "umh1".FromRawcode();
            yield return "umh2".FromRawcode();
            yield return "ua1z".FromRawcode();
            yield return "ua2z".FromRawcode();
            yield return "umsl".FromRawcode();
            yield return "unam".FromRawcode();
            yield return "upro".FromRawcode();
            yield return "ursl".FromRawcode();
            yield return "urqc".FromRawcode();
            yield return "ureq".FromRawcode();
            yield return "urq1".FromRawcode();
            yield return "urq2".FromRawcode();
            yield return "urq3".FromRawcode();
            yield return "urq4".FromRawcode();
            yield return "urq5".FromRawcode();
            yield return "urq6".FromRawcode();
            yield return "urq7".FromRawcode();
            yield return "urq8".FromRawcode();
            yield return "urqa".FromRawcode();
            yield return "ures".FromRawcode();
            yield return "urev".FromRawcode();
            yield return "utpr".FromRawcode();
            yield return "ussi".FromRawcode();
            yield return "usei".FromRawcode();
            yield return "useu".FromRawcode();
            yield return "uspa".FromRawcode();
            yield return "utaa".FromRawcode();
            yield return "utip".FromRawcode();
            yield return "utra".FromRawcode();
            yield return "urva".FromRawcode();
            yield return "utub".FromRawcode();
            yield return "uupt".FromRawcode();
            yield return "uabi".FromRawcode();
            yield return "udaa".FromRawcode();
            yield return "uhab".FromRawcode();
            yield return "uagi".FromRawcode();
            yield return "uagp".FromRawcode();
            yield return "ubld".FromRawcode();
            yield return "ubdi".FromRawcode();
            yield return "ubba".FromRawcode();
            yield return "ubsi".FromRawcode();
            yield return "ulbd".FromRawcode();
            yield return "ulba".FromRawcode();
            yield return "ulbs".FromRawcode();
            yield return "ucol".FromRawcode();
            yield return "udef".FromRawcode();
            yield return "udty".FromRawcode();
            yield return "udup".FromRawcode();
            yield return "ufma".FromRawcode();
            yield return "ufoo".FromRawcode();
            yield return "ugol".FromRawcode();
            yield return "ugor".FromRawcode();
            yield return "uhpm".FromRawcode();
            yield return "uint".FromRawcode();
            yield return "uinp".FromRawcode();
            yield return "ubdg".FromRawcode();
            yield return "ulev".FromRawcode();
            yield return "ulum".FromRawcode();
            yield return "ulur".FromRawcode();
            yield return "umpi".FromRawcode();
            yield return "umpm".FromRawcode();
            yield return "umas".FromRawcode();
            yield return "umis".FromRawcode();
            yield return "unbr".FromRawcode();
            yield return "usin".FromRawcode();
            yield return "upap".FromRawcode();
            yield return "upra".FromRawcode();
            yield return "uhpr".FromRawcode();
            yield return "umpr".FromRawcode();
            yield return "uhrt".FromRawcode();
            yield return "urtm".FromRawcode();
            yield return "urpo".FromRawcode();
            yield return "urpg".FromRawcode();
            yield return "urpp".FromRawcode();
            yield return "urpr".FromRawcode();
            yield return "upar".FromRawcode();
            yield return "usid".FromRawcode();
            yield return "umvs".FromRawcode();
            yield return "usma".FromRawcode();
            yield return "usrg".FromRawcode();
            yield return "usst".FromRawcode();
            yield return "ustr".FromRawcode();
            yield return "ustp".FromRawcode();
            yield return "util".FromRawcode();
            yield return "utyp".FromRawcode();
            yield return "upgr".FromRawcode();
            yield return "uabr".FromRawcode();
            yield return "uabt".FromRawcode();
            yield return "ucbo".FromRawcode();
            yield return "ufle".FromRawcode();
            yield return "usle".FromRawcode();
            yield return "ucar".FromRawcode();
            yield return "udtm".FromRawcode();
            yield return "udea".FromRawcode();
            yield return "ulos".FromRawcode();
            yield return "ufor".FromRawcode();
            yield return "uibo".FromRawcode();
            yield return "umvf".FromRawcode();
            yield return "umvh".FromRawcode();
            yield return "umvt".FromRawcode();
            yield return "upru".FromRawcode();
            yield return "uori".FromRawcode();
            yield return "upat".FromRawcode();
            yield return "upoi".FromRawcode();
            yield return "upri".FromRawcode();
            yield return "uprw".FromRawcode();
            yield return "urac".FromRawcode();
            yield return "upaw".FromRawcode();
            yield return "utar".FromRawcode();
            yield return "umvr".FromRawcode();
            yield return "uarm".FromRawcode();
            yield return "uble".FromRawcode();
            yield return "uclb".FromRawcode();
            yield return "ushb".FromRawcode();
            yield return "ucam".FromRawcode();
            yield return "utcc".FromRawcode();
            yield return "udro".FromRawcode();
            yield return "uept".FromRawcode();
            yield return "uerd".FromRawcode();
            yield return "umdl".FromRawcode();
            yield return "uver".FromRawcode();
            yield return "ufrd".FromRawcode();
            yield return "uclg".FromRawcode();
            yield return "uhos".FromRawcode();
            yield return "uine".FromRawcode();
            yield return "umxp".FromRawcode();
            yield return "umxr".FromRawcode();
            yield return "usca".FromRawcode();
            yield return "unbm".FromRawcode();
            yield return "uhhb".FromRawcode();
            yield return "uhhm".FromRawcode();
            yield return "uhhd".FromRawcode();
            yield return "uhom".FromRawcode();
            yield return "uocc".FromRawcode();
            yield return "uclr".FromRawcode();
            yield return "urun".FromRawcode();
            yield return "ussc".FromRawcode();
            yield return "uscb".FromRawcode();
            yield return "usew".FromRawcode();
            yield return "uslz".FromRawcode();
            yield return "ushh".FromRawcode();
            yield return "ushr".FromRawcode();
            yield return "ushw".FromRawcode();
            yield return "ushx".FromRawcode();
            yield return "ushy".FromRawcode();
            yield return "uspe".FromRawcode();
            yield return "utco".FromRawcode();
            yield return "utss".FromRawcode();
            yield return "uubs".FromRawcode();
            yield return "ushu".FromRawcode();
            yield return "usnd".FromRawcode();
            yield return "uuch".FromRawcode();
            yield return "uwal".FromRawcode();
            yield return "uacq".FromRawcode();
            yield return "ua1t".FromRawcode();
            yield return "ua2t".FromRawcode();
            yield return "ubs1".FromRawcode();
            yield return "ubs2".FromRawcode();
            yield return "ucbs".FromRawcode();
            yield return "ucpt".FromRawcode();
            yield return "ua1c".FromRawcode();
            yield return "ua2c".FromRawcode();
            yield return "udl1".FromRawcode();
            yield return "udl2".FromRawcode();
            yield return "ua1d".FromRawcode();
            yield return "ua2d".FromRawcode();
            yield return "ua1b".FromRawcode();
            yield return "ua2b".FromRawcode();
            yield return "udp1".FromRawcode();
            yield return "udp2".FromRawcode();
            yield return "udu1".FromRawcode();
            yield return "udu2".FromRawcode();
            yield return "ua1f".FromRawcode();
            yield return "ua2f".FromRawcode();
            yield return "ua1h".FromRawcode();
            yield return "ua2h".FromRawcode();
            yield return "uhd1".FromRawcode();
            yield return "uhd2".FromRawcode();
            yield return "uisz".FromRawcode();
            yield return "uimz".FromRawcode();
            yield return "ulsz".FromRawcode();
            yield return "ulpx".FromRawcode();
            yield return "ulpy".FromRawcode();
            yield return "ulpz".FromRawcode();
            yield return "uamn".FromRawcode();
            yield return "ua1q".FromRawcode();
            yield return "ua2q".FromRawcode();
            yield return "uqd1".FromRawcode();
            yield return "uqd2".FromRawcode();
            yield return "ua1r".FromRawcode();
            yield return "ua2r".FromRawcode();
            yield return "urb1".FromRawcode();
            yield return "urb2".FromRawcode();
            yield return "uwu1".FromRawcode();
            yield return "uwu2".FromRawcode();
            yield return "ua1s".FromRawcode();
            yield return "ua2s".FromRawcode();
            yield return "usd1".FromRawcode();
            yield return "usd2".FromRawcode();
            yield return "usr1".FromRawcode();
            yield return "usr2".FromRawcode();
            yield return "ua1p".FromRawcode();
            yield return "ua2p".FromRawcode();
            yield return "utc1".FromRawcode();
            yield return "utc2".FromRawcode();
            yield return "ua1g".FromRawcode();
            yield return "ua2g".FromRawcode();
            yield return "uaen".FromRawcode();
            yield return "ua1w".FromRawcode();
            yield return "ua2w".FromRawcode();
            yield return "ucs1".FromRawcode();
            yield return "ucs2".FromRawcode();
        }

        private static IEnumerable<int> GetPropertyRawcodesPatch1_31_1()
        {
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