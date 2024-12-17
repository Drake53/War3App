using System;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Extensions
{
    public static class GamePatchExtensions
    {
        public static int GetEditorVersion(this GamePatch gamePatch)
        {
            // https://www.hiveworkshop.com/threads/list-of-official-patches-for-warcraft-3.322919/
            switch (gamePatch)
            {
                case GamePatch.v1_00:
                    return 4448;

                case GamePatch.v1_01:
                case GamePatch.v1_01b:
                case GamePatch.v1_01c:
                    return 4482;

                case GamePatch.v1_02:
                case GamePatch.v1_02a:
                    return 4531;

                case GamePatch.v1_03:
                    return 4572;

                case GamePatch.v1_04:
                case GamePatch.v1_04b:
                case GamePatch.v1_04c:
                case GamePatch.v1_05:
                case GamePatch.v1_06:
                    return 4654;

                case GamePatch.v1_07:
                    return 6031;

                case GamePatch.v1_10:
                    return 6034;

                case GamePatch.v1_11:
                    return 6035;

                case GamePatch.v1_12:
                    return 6036;

                case GamePatch.v1_13:
                case GamePatch.v1_13b:
                    return 6037;

                case GamePatch.v1_14:
                    return 6039;

                case GamePatch.v1_14b:
                    return 6040;

                case GamePatch.v1_15:
                    return 6043;

                case GamePatch.v1_16:
                    return 6046;

                case GamePatch.v1_17:
                    return 6050;

                case GamePatch.v1_18:
                    return 6051;

                case GamePatch.v1_19:
                case GamePatch.v1_19b:
                case GamePatch.v1_20a:
                case GamePatch.v1_20b:
                case GamePatch.v1_20c:
                case GamePatch.v1_20d:
                case GamePatch.v1_20e:
                case GamePatch.v1_21:
                case GamePatch.v1_21b:
                    return 6052;

                case GamePatch.v1_22:
                    return 6057;

                case GamePatch.v1_23:
                    return 6058;

                case GamePatch.v1_24a:
                case GamePatch.v1_24b:
                case GamePatch.v1_24c:
                case GamePatch.v1_24d:
                case GamePatch.v1_24e:
                case GamePatch.v1_25b:
                case GamePatch.v1_26a:
                case GamePatch.v1_27a:
                case GamePatch.v1_27b:
                case GamePatch.v1_28:
                case GamePatch.v1_28_1:
                case GamePatch.v1_28_2:
                case GamePatch.v1_28_3:
                case GamePatch.v1_28_4:
                case GamePatch.v1_28_5:
                    return 6059;

                case GamePatch.v1_29_0:
                case GamePatch.v1_29_1:
                case GamePatch.v1_29_2:
                    return 6060;

                case GamePatch.v1_30_0:
                case GamePatch.v1_30_1:
                case GamePatch.v1_30_2:
                case GamePatch.v1_30_3:
                case GamePatch.v1_30_4:
                    return 6061;

                case GamePatch.v1_31_0:
                case GamePatch.v1_31_1:
                    return 6072;

                case GamePatch.v1_32_0:
                case GamePatch.v1_32_1:
                    return 6105;

                case GamePatch.v1_32_2:
                    return 6106;

                // TODO
                case GamePatch.v1_32_3:
                case GamePatch.v1_32_4:
                case GamePatch.v1_32_5:
                case GamePatch.v1_32_6:
                case GamePatch.v1_32_7:
                case GamePatch.v1_32_8:
                case GamePatch.v1_32_9:
                case GamePatch.v1_32_10:
                    return 6106;

                default: throw new NotSupportedException();
            };
        }

        public static string PrettyPrint(this GamePatch gamePatch)
        {
            return gamePatch.ToString().Replace('_', '.');
        }
    }
}