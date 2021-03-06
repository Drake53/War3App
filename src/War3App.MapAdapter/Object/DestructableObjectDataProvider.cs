﻿using System;
using System.Collections.Generic;

using War3Net.Build.Common;
using War3Net.Common.Extensions;

namespace War3App.MapAdapter.Object
{
    public static class DestructableObjectDataProvider
    {
        public static IEnumerable<int> GetRawcodes(GamePatch patch)
        {
            switch (patch)
            {
                case GamePatch.v1_28:
                case GamePatch.v1_28_1:
                case GamePatch.v1_28_2:
                case GamePatch.v1_28_3:
                case GamePatch.v1_28_4:
                case GamePatch.v1_28_5:
                // TODO

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
                case GamePatch.v1_28:
                case GamePatch.v1_28_1:
                case GamePatch.v1_28_2:
                case GamePatch.v1_28_3:
                case GamePatch.v1_28_4:
                case GamePatch.v1_28_5:
                // TODO

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
            yield return "ATtr".FromRawcode();
            yield return "BTtw".FromRawcode();
            yield return "CTtr".FromRawcode();
            yield return "FTtw".FromRawcode();
            yield return "LOcg".FromRawcode();
            yield return "LTba".FromRawcode();
            yield return "LTcr".FromRawcode();
            yield return "LTbr".FromRawcode();
            yield return "LTbx".FromRawcode();
            yield return "LTbs".FromRawcode();
            yield return "LTex".FromRawcode();
            yield return "LTg1".FromRawcode();
            yield return "LTg2".FromRawcode();
            yield return "LTg3".FromRawcode();
            yield return "LTg4".FromRawcode();
            yield return "LTe1".FromRawcode();
            yield return "LTe2".FromRawcode();
            yield return "LTe3".FromRawcode();
            yield return "LTe4".FromRawcode();
            yield return "ATg1".FromRawcode();
            yield return "ATg2".FromRawcode();
            yield return "ATg3".FromRawcode();
            yield return "ATg4".FromRawcode();
            yield return "DTg5".FromRawcode();
            yield return "DTg6".FromRawcode();
            yield return "DTg7".FromRawcode();
            yield return "DTg8".FromRawcode();
            yield return "DTg1".FromRawcode();
            yield return "DTg2".FromRawcode();
            yield return "DTg3".FromRawcode();
            yield return "DTg4".FromRawcode();
            yield return "LTlt".FromRawcode();
            yield return "NTtw".FromRawcode();
            yield return "WTtw".FromRawcode();
            yield return "WTst".FromRawcode();
            yield return "YTct".FromRawcode();
            yield return "YTwt".FromRawcode();
            yield return "YTst".FromRawcode();
            yield return "YTft".FromRawcode();
            yield return "VTlt".FromRawcode();
            yield return "LTw0".FromRawcode();
            yield return "LTw1".FromRawcode();
            yield return "LTw2".FromRawcode();
            yield return "LTw3".FromRawcode();
            yield return "YT00".FromRawcode();
            yield return "YT01".FromRawcode();
            yield return "YT02".FromRawcode();
            yield return "YT03".FromRawcode();
            yield return "YT04".FromRawcode();
            yield return "YT05".FromRawcode();
            yield return "YT06".FromRawcode();
            yield return "YT07".FromRawcode();
            yield return "YT08".FromRawcode();
            yield return "YT09".FromRawcode();
            yield return "YT10".FromRawcode();
            yield return "YT11".FromRawcode();
            yield return "YT12".FromRawcode();
            yield return "YT13".FromRawcode();
            yield return "YT14".FromRawcode();
            yield return "YT15".FromRawcode();
            yield return "YT16".FromRawcode();
            yield return "YT17".FromRawcode();
            yield return "YT18".FromRawcode();
            yield return "YT19".FromRawcode();
            yield return "YT20".FromRawcode();
            yield return "YT21".FromRawcode();
            yield return "YT22".FromRawcode();
            yield return "YT23".FromRawcode();
            yield return "LT00".FromRawcode();
            yield return "LT01".FromRawcode();
            yield return "LT02".FromRawcode();
            yield return "LT03".FromRawcode();
            yield return "LT04".FromRawcode();
            yield return "LT05".FromRawcode();
            yield return "LT06".FromRawcode();
            yield return "LT07".FromRawcode();
            yield return "LT08".FromRawcode();
            yield return "LT09".FromRawcode();
            yield return "LT10".FromRawcode();
            yield return "LT11".FromRawcode();
            yield return "XTbd".FromRawcode();
            yield return "XTvt".FromRawcode();
            yield return "LTr1".FromRawcode();
            yield return "LTr2".FromRawcode();
            yield return "LTr3".FromRawcode();
            yield return "LTr4".FromRawcode();
            yield return "LTr5".FromRawcode();
            yield return "LTr6".FromRawcode();
            yield return "LTr7".FromRawcode();
            yield return "LTr8".FromRawcode();
            yield return "NTbd".FromRawcode();
            yield return "DTes".FromRawcode();
            yield return "DTsh".FromRawcode();
            yield return "YSdb".FromRawcode();
            yield return "YSdc".FromRawcode();
            yield return "XOkt".FromRawcode();
            yield return "XOk1".FromRawcode();
            yield return "XOk2".FromRawcode();
            yield return "DTc1".FromRawcode();
            yield return "DTc2".FromRawcode();
            yield return "DTsp".FromRawcode();
            yield return "DTrc".FromRawcode();
            yield return "DTsb".FromRawcode();
            yield return "DTs1".FromRawcode();
            yield return "DTs2".FromRawcode();
            yield return "DTs3".FromRawcode();
            yield return "Dofw".FromRawcode();
            yield return "Dofv".FromRawcode();
            yield return "YT24".FromRawcode();
            yield return "YT25".FromRawcode();
            yield return "YT26".FromRawcode();
            yield return "YT27".FromRawcode();
            yield return "YT28".FromRawcode();
            yield return "YT29".FromRawcode();
            yield return "YT30".FromRawcode();
            yield return "YT31".FromRawcode();
            yield return "YT32".FromRawcode();
            yield return "YT33".FromRawcode();
            yield return "YT34".FromRawcode();
            yield return "YT35".FromRawcode();
            yield return "YT36".FromRawcode();
            yield return "YT37".FromRawcode();
            yield return "YT38".FromRawcode();
            yield return "YT39".FromRawcode();
            yield return "YT40".FromRawcode();
            yield return "YT41".FromRawcode();
            yield return "YT42".FromRawcode();
            yield return "YT43".FromRawcode();
            yield return "YT44".FromRawcode();
            yield return "YT45".FromRawcode();
            yield return "YT46".FromRawcode();
            yield return "YT47".FromRawcode();
            yield return "ZTr0".FromRawcode();
            yield return "ZTr1".FromRawcode();
            yield return "ZTr2".FromRawcode();
            yield return "ZTr3".FromRawcode();
            yield return "ZTtw".FromRawcode();
            yield return "ZTw0".FromRawcode();
            yield return "ZTw1".FromRawcode();
            yield return "ZTw2".FromRawcode();
            yield return "ZTw3".FromRawcode();
            yield return "ZTg1".FromRawcode();
            yield return "ZTg2".FromRawcode();
            yield return "ZTg3".FromRawcode();
            yield return "ZTg4".FromRawcode();
            yield return "ITtw".FromRawcode();
            yield return "ZTd1".FromRawcode();
            yield return "ZTd2".FromRawcode();
            yield return "ZTd3".FromRawcode();
            yield return "ZTd4".FromRawcode();
            yield return "ZTd5".FromRawcode();
            yield return "ZTd6".FromRawcode();
            yield return "ZTd7".FromRawcode();
            yield return "ZTd8".FromRawcode();
            yield return "ITib".FromRawcode();
            yield return "ITi2".FromRawcode();
            yield return "ITi3".FromRawcode();
            yield return "ITi4".FromRawcode();
            yield return "ITg1".FromRawcode();
            yield return "ITg2".FromRawcode();
            yield return "ITg3".FromRawcode();
            yield return "ITg4".FromRawcode();
            yield return "ITw0".FromRawcode();
            yield return "ITw1".FromRawcode();
            yield return "ITw2".FromRawcode();
            yield return "ITw3".FromRawcode();
            yield return "LTt0".FromRawcode();
            yield return "LTt1".FromRawcode();
            yield return "LTt2".FromRawcode();
            yield return "LTt3".FromRawcode();
            yield return "LTt4".FromRawcode();
            yield return "ATt0".FromRawcode();
            yield return "ATt1".FromRawcode();
            yield return "LTt5".FromRawcode();
            yield return "ZTnc".FromRawcode();
            yield return "ITf1".FromRawcode();
            yield return "ITf2".FromRawcode();
            yield return "ITf3".FromRawcode();
            yield return "ITf4".FromRawcode();
            yield return "ITx1".FromRawcode();
            yield return "ITx2".FromRawcode();
            yield return "ITx3".FromRawcode();
            yield return "ITx4".FromRawcode();
            yield return "ATtc".FromRawcode();
            yield return "OTtw".FromRawcode();
            yield return "KTtw".FromRawcode();
            yield return "ITig".FromRawcode();
            yield return "DTrf".FromRawcode();
            yield return "DTrx".FromRawcode();
            yield return "XTmp".FromRawcode();
            yield return "XTm5".FromRawcode();
            yield return "XTmx".FromRawcode();
            yield return "XTx5".FromRawcode();
            yield return "ITcr".FromRawcode();
            yield return "DTep".FromRawcode();
            yield return "ATwf".FromRawcode();
            yield return "YTfb".FromRawcode();
            yield return "YTfc".FromRawcode();
            yield return "YTlb".FromRawcode();
            yield return "Ytlc".FromRawcode();
            yield return "YTpb".FromRawcode();
            yield return "YTpc".FromRawcode();
            yield return "YTab".FromRawcode();
            yield return "YTac".FromRawcode();
            yield return "ZTsg".FromRawcode();
            yield return "ZTsx".FromRawcode();
            yield return "DTfp".FromRawcode();
            yield return "DTfx".FromRawcode();
            yield return "DTlv".FromRawcode();
            yield return "YTce".FromRawcode();
            yield return "YTcx".FromRawcode();
            yield return "LTtc".FromRawcode();
            yield return "LTtx".FromRawcode();
            yield return "JTct".FromRawcode();
            yield return "JTtw".FromRawcode();
            yield return "ITtg".FromRawcode();
            yield return "GTsh".FromRawcode();
            yield return "BTrs".FromRawcode();
            yield return "BTrx".FromRawcode();
            yield return "OTsp".FromRawcode();
            yield return "OTip".FromRawcode();
            yield return "OTis".FromRawcode();
            yield return "BTtc".FromRawcode();
            yield return "CTtc".FromRawcode();
            yield return "NTtc".FromRawcode();
            yield return "ZTtc".FromRawcode();
            yield return "ITtc".FromRawcode();
            yield return "IOt0".FromRawcode();
            yield return "IOt1".FromRawcode();
            yield return "IOt2".FromRawcode();
            yield return "LTrc".FromRawcode();
            yield return "YT48".FromRawcode();
            yield return "YT49".FromRawcode();
            yield return "YT50".FromRawcode();
            yield return "YT51".FromRawcode();
            yield return "OTds".FromRawcode();
            yield return "ITag".FromRawcode();
            yield return "BTsc".FromRawcode();
            yield return "LTs1".FromRawcode();
            yield return "LTs2".FromRawcode();
            yield return "LTs3".FromRawcode();
            yield return "LTs4".FromRawcode();
            yield return "LTs5".FromRawcode();
            yield return "LTs6".FromRawcode();
            yield return "LTs7".FromRawcode();
            yield return "LTs8".FromRawcode();
            yield return "Volc".FromRawcode();
        }

        private static IEnumerable<int> GetRawcodesPatch1_31_1_12173()
        {
            yield return "ATtr".FromRawcode();
            yield return "BTtw".FromRawcode();
            yield return "CTtr".FromRawcode();
            yield return "FTtw".FromRawcode();
            yield return "LOcg".FromRawcode();
            yield return "LTba".FromRawcode();
            yield return "LTcr".FromRawcode();
            yield return "LTbr".FromRawcode();
            yield return "LTbx".FromRawcode();
            yield return "LTbs".FromRawcode();
            yield return "LTex".FromRawcode();
            yield return "LTg1".FromRawcode();
            yield return "LTg2".FromRawcode();
            yield return "LTg3".FromRawcode();
            yield return "LTg4".FromRawcode();
            yield return "LTe1".FromRawcode();
            yield return "LTe2".FromRawcode();
            yield return "LTe3".FromRawcode();
            yield return "LTe4".FromRawcode();
            yield return "ATg1".FromRawcode();
            yield return "ATg2".FromRawcode();
            yield return "ATg3".FromRawcode();
            yield return "ATg4".FromRawcode();
            yield return "DTg5".FromRawcode();
            yield return "DTg6".FromRawcode();
            yield return "DTg7".FromRawcode();
            yield return "DTg8".FromRawcode();
            yield return "DTg1".FromRawcode();
            yield return "DTg2".FromRawcode();
            yield return "DTg3".FromRawcode();
            yield return "DTg4".FromRawcode();
            yield return "LTlt".FromRawcode();
            yield return "NTtw".FromRawcode();
            yield return "WTtw".FromRawcode();
            yield return "WTst".FromRawcode();
            yield return "YTct".FromRawcode();
            yield return "YTwt".FromRawcode();
            yield return "YTst".FromRawcode();
            yield return "YTft".FromRawcode();
            yield return "VTlt".FromRawcode();
            yield return "LTw0".FromRawcode();
            yield return "LTw1".FromRawcode();
            yield return "LTw2".FromRawcode();
            yield return "LTw3".FromRawcode();
            yield return "YT00".FromRawcode();
            yield return "YT01".FromRawcode();
            yield return "YT02".FromRawcode();
            yield return "YT03".FromRawcode();
            yield return "YT04".FromRawcode();
            yield return "YT05".FromRawcode();
            yield return "YT06".FromRawcode();
            yield return "YT07".FromRawcode();
            yield return "YT08".FromRawcode();
            yield return "YT09".FromRawcode();
            yield return "YT10".FromRawcode();
            yield return "YT11".FromRawcode();
            yield return "YT12".FromRawcode();
            yield return "YT13".FromRawcode();
            yield return "YT14".FromRawcode();
            yield return "YT15".FromRawcode();
            yield return "YT16".FromRawcode();
            yield return "YT17".FromRawcode();
            yield return "YT18".FromRawcode();
            yield return "YT19".FromRawcode();
            yield return "YT20".FromRawcode();
            yield return "YT21".FromRawcode();
            yield return "YT22".FromRawcode();
            yield return "YT23".FromRawcode();
            yield return "LT00".FromRawcode();
            yield return "LT01".FromRawcode();
            yield return "LT02".FromRawcode();
            yield return "LT03".FromRawcode();
            yield return "LT04".FromRawcode();
            yield return "LT05".FromRawcode();
            yield return "LT06".FromRawcode();
            yield return "LT07".FromRawcode();
            yield return "LT08".FromRawcode();
            yield return "LT09".FromRawcode();
            yield return "LT10".FromRawcode();
            yield return "LT11".FromRawcode();
            yield return "XTbd".FromRawcode();
            yield return "XTvt".FromRawcode();
            yield return "LTr1".FromRawcode();
            yield return "LTr2".FromRawcode();
            yield return "LTr3".FromRawcode();
            yield return "LTr4".FromRawcode();
            yield return "LTr5".FromRawcode();
            yield return "LTr6".FromRawcode();
            yield return "LTr7".FromRawcode();
            yield return "LTr8".FromRawcode();
            yield return "NTbd".FromRawcode();
            yield return "DTes".FromRawcode();
            yield return "DTsh".FromRawcode();
            yield return "YSdb".FromRawcode();
            yield return "YSdc".FromRawcode();
            yield return "XOkt".FromRawcode();
            yield return "XOk1".FromRawcode();
            yield return "XOk2".FromRawcode();
            yield return "DTc1".FromRawcode();
            yield return "DTc2".FromRawcode();
            yield return "DTsp".FromRawcode();
            yield return "DTrc".FromRawcode();
            yield return "DTsb".FromRawcode();
            yield return "DTs1".FromRawcode();
            yield return "DTs2".FromRawcode();
            yield return "DTs3".FromRawcode();
            yield return "Dofw".FromRawcode();
            yield return "Dofv".FromRawcode();
            yield return "YT24".FromRawcode();
            yield return "YT25".FromRawcode();
            yield return "YT26".FromRawcode();
            yield return "YT27".FromRawcode();
            yield return "YT28".FromRawcode();
            yield return "YT29".FromRawcode();
            yield return "YT30".FromRawcode();
            yield return "YT31".FromRawcode();
            yield return "YT32".FromRawcode();
            yield return "YT33".FromRawcode();
            yield return "YT34".FromRawcode();
            yield return "YT35".FromRawcode();
            yield return "YT36".FromRawcode();
            yield return "YT37".FromRawcode();
            yield return "YT38".FromRawcode();
            yield return "YT39".FromRawcode();
            yield return "YT40".FromRawcode();
            yield return "YT41".FromRawcode();
            yield return "YT42".FromRawcode();
            yield return "YT43".FromRawcode();
            yield return "YT44".FromRawcode();
            yield return "YT45".FromRawcode();
            yield return "YT46".FromRawcode();
            yield return "YT47".FromRawcode();
            yield return "ZTr0".FromRawcode();
            yield return "ZTr1".FromRawcode();
            yield return "ZTr2".FromRawcode();
            yield return "ZTr3".FromRawcode();
            yield return "ZTtw".FromRawcode();
            yield return "ZTw0".FromRawcode();
            yield return "ZTw1".FromRawcode();
            yield return "ZTw2".FromRawcode();
            yield return "ZTw3".FromRawcode();
            yield return "ZTg1".FromRawcode();
            yield return "ZTg2".FromRawcode();
            yield return "ZTg3".FromRawcode();
            yield return "ZTg4".FromRawcode();
            yield return "ITtw".FromRawcode();
            yield return "ZTd1".FromRawcode();
            yield return "ZTd2".FromRawcode();
            yield return "ZTd3".FromRawcode();
            yield return "ZTd4".FromRawcode();
            yield return "ZTd5".FromRawcode();
            yield return "ZTd6".FromRawcode();
            yield return "ZTd7".FromRawcode();
            yield return "ZTd8".FromRawcode();
            yield return "ITib".FromRawcode();
            yield return "ITi2".FromRawcode();
            yield return "ITi3".FromRawcode();
            yield return "ITi4".FromRawcode();
            yield return "ITg1".FromRawcode();
            yield return "ITg2".FromRawcode();
            yield return "ITg3".FromRawcode();
            yield return "ITg4".FromRawcode();
            yield return "ITw0".FromRawcode();
            yield return "ITw1".FromRawcode();
            yield return "ITw2".FromRawcode();
            yield return "ITw3".FromRawcode();
            yield return "LTt0".FromRawcode();
            yield return "LTt1".FromRawcode();
            yield return "LTt2".FromRawcode();
            yield return "LTt3".FromRawcode();
            yield return "LTt4".FromRawcode();
            yield return "ATt0".FromRawcode();
            yield return "ATt1".FromRawcode();
            yield return "LTt5".FromRawcode();
            yield return "ZTnc".FromRawcode();
            yield return "ITf1".FromRawcode();
            yield return "ITf2".FromRawcode();
            yield return "ITf3".FromRawcode();
            yield return "ITf4".FromRawcode();
            yield return "ITx1".FromRawcode();
            yield return "ITx2".FromRawcode();
            yield return "ITx3".FromRawcode();
            yield return "ITx4".FromRawcode();
            yield return "ATtc".FromRawcode();
            yield return "OTtw".FromRawcode();
            yield return "KTtw".FromRawcode();
            yield return "ITig".FromRawcode();
            yield return "DTrf".FromRawcode();
            yield return "DTrx".FromRawcode();
            yield return "XTmp".FromRawcode();
            yield return "XTm5".FromRawcode();
            yield return "XTmx".FromRawcode();
            yield return "XTx5".FromRawcode();
            yield return "ITcr".FromRawcode();
            yield return "DTep".FromRawcode();
            yield return "ATwf".FromRawcode();
            yield return "YTfb".FromRawcode();
            yield return "YTfc".FromRawcode();
            yield return "YTlb".FromRawcode();
            yield return "Ytlc".FromRawcode();
            yield return "YTpb".FromRawcode();
            yield return "YTpc".FromRawcode();
            yield return "YTab".FromRawcode();
            yield return "YTac".FromRawcode();
            yield return "ZTsg".FromRawcode();
            yield return "ZTsx".FromRawcode();
            yield return "DTfp".FromRawcode();
            yield return "DTfx".FromRawcode();
            yield return "DTlv".FromRawcode();
            yield return "YTce".FromRawcode();
            yield return "YTcx".FromRawcode();
            yield return "LTtc".FromRawcode();
            yield return "LTtx".FromRawcode();
            yield return "JTct".FromRawcode();
            yield return "JTtw".FromRawcode();
            yield return "ITtg".FromRawcode();
            yield return "GTsh".FromRawcode();
            yield return "BTrs".FromRawcode();
            yield return "BTrx".FromRawcode();
            yield return "OTsp".FromRawcode();
            yield return "OTip".FromRawcode();
            yield return "OTis".FromRawcode();
            yield return "BTtc".FromRawcode();
            yield return "CTtc".FromRawcode();
            yield return "NTtc".FromRawcode();
            yield return "ZTtc".FromRawcode();
            yield return "ITtc".FromRawcode();
            yield return "IOt0".FromRawcode();
            yield return "IOt1".FromRawcode();
            yield return "IOt2".FromRawcode();
            yield return "LTrc".FromRawcode();
            yield return "YT48".FromRawcode();
            yield return "YT49".FromRawcode();
            yield return "YT50".FromRawcode();
            yield return "YT51".FromRawcode();
            yield return "OTds".FromRawcode();
            yield return "ITag".FromRawcode();
            yield return "BTsc".FromRawcode();
            yield return "LTs1".FromRawcode();
            yield return "LTs2".FromRawcode();
            yield return "LTs3".FromRawcode();
            yield return "LTs4".FromRawcode();
            yield return "LTs5".FromRawcode();
            yield return "LTs6".FromRawcode();
            yield return "LTs7".FromRawcode();
            yield return "LTs8".FromRawcode();
            yield return "Volc".FromRawcode();
        }

        private static IEnumerable<int> GetPropertyRawcodesPatch1_29_2()
        {
            yield return "bnam".FromRawcode();
            yield return "bsuf".FromRawcode();
            yield return "bcat".FromRawcode();
            yield return "btil".FromRawcode();
            yield return "btsp".FromRawcode();
            yield return "bfil".FromRawcode();
            yield return "blit".FromRawcode();
            yield return "bflo".FromRawcode();
            yield return "btxi".FromRawcode();
            yield return "btxf".FromRawcode();
            yield return "buch".FromRawcode();
            yield return "bonc".FromRawcode();
            yield return "bonw".FromRawcode();
            yield return "bcpd".FromRawcode();
            yield return "bwal".FromRawcode();
            yield return "bclh".FromRawcode();
            yield return "btar".FromRawcode();
            yield return "barm".FromRawcode();
            yield return "bvar".FromRawcode();
            yield return "bhps".FromRawcode();
            yield return "boch".FromRawcode();
            yield return "bflh".FromRawcode();
            yield return "bfxr".FromRawcode();
            yield return "bsel".FromRawcode();
            yield return "bmis".FromRawcode();
            yield return "bmas".FromRawcode();
            yield return "bcpr".FromRawcode();
            yield return "bmap".FromRawcode();
            yield return "bmar".FromRawcode();
            yield return "brad".FromRawcode();
            yield return "bfra".FromRawcode();
            yield return "bfvi".FromRawcode();
            yield return "bptx".FromRawcode();
            yield return "bptd".FromRawcode();
            yield return "bdsn".FromRawcode();
            yield return "bshd".FromRawcode();
            yield return "bsmm".FromRawcode();
            yield return "bmmr".FromRawcode();
            yield return "bmmg".FromRawcode();
            yield return "bmmb".FromRawcode();
            yield return "bumm".FromRawcode();
            yield return "bbut".FromRawcode();
            yield return "bret".FromRawcode();
            yield return "breg".FromRawcode();
            yield return "brel".FromRawcode();
            yield return "busr".FromRawcode();
            yield return "bvcr".FromRawcode();
            yield return "bvcg".FromRawcode();
            yield return "bvcb".FromRawcode();
            yield return "bgse".FromRawcode();
            yield return "bgsc".FromRawcode();
            yield return "bgpm".FromRawcode();
        }

        private static IEnumerable<int> GetPropertyRawcodesPatch1_31_1_12173()
        {
            yield return "bnam".FromRawcode();
            yield return "bsuf".FromRawcode();
            yield return "bcat".FromRawcode();
            yield return "btil".FromRawcode();
            yield return "btsp".FromRawcode();
            yield return "bfil".FromRawcode();
            yield return "blit".FromRawcode();
            yield return "bflo".FromRawcode();
            yield return "btxi".FromRawcode();
            yield return "btxf".FromRawcode();
            yield return "buch".FromRawcode();
            yield return "bonc".FromRawcode();
            yield return "bonw".FromRawcode();
            yield return "bcpd".FromRawcode();
            yield return "bwal".FromRawcode();
            yield return "bclh".FromRawcode();
            yield return "btar".FromRawcode();
            yield return "barm".FromRawcode();
            yield return "bvar".FromRawcode();
            yield return "bhps".FromRawcode();
            yield return "boch".FromRawcode();
            yield return "bflh".FromRawcode();
            yield return "bfxr".FromRawcode();
            yield return "bsel".FromRawcode();
            yield return "bmis".FromRawcode();
            yield return "bmas".FromRawcode();
            yield return "bcpr".FromRawcode();
            yield return "bmap".FromRawcode();
            yield return "bmar".FromRawcode();
            yield return "brad".FromRawcode();
            yield return "bfra".FromRawcode();
            yield return "bfvi".FromRawcode();
            yield return "bptx".FromRawcode();
            yield return "bptd".FromRawcode();
            yield return "bdsn".FromRawcode();
            yield return "bshd".FromRawcode();
            yield return "bsmm".FromRawcode();
            yield return "bmmr".FromRawcode();
            yield return "bmmg".FromRawcode();
            yield return "bmmb".FromRawcode();
            yield return "bumm".FromRawcode();
            yield return "bbut".FromRawcode();
            yield return "bret".FromRawcode();
            yield return "breg".FromRawcode();
            yield return "brel".FromRawcode();
            yield return "busr".FromRawcode();
            yield return "bvcr".FromRawcode();
            yield return "bvcg".FromRawcode();
            yield return "bvcb".FromRawcode();
            yield return "bgse".FromRawcode();
            yield return "bgsc".FromRawcode();
            yield return "bgpm".FromRawcode();
        }
    }
}