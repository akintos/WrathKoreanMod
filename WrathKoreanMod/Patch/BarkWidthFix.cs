using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WrathKoreanMod.Patch
{
    internal class BarkWidthFix
    {
        [HarmonyPatch(typeof(Kingmaker.UI.Common.UIUtility), "CalculateBarkWidth")]
        private static class UIUtility_CalculateBarkWidth_Patch
        {
            public static bool Prefix(string text, float symWidth, ref float __result)
            {
                if (Main.Enabled)
                {
                    __result = MyCalculateBarkWidth(text, symWidth);
                    return false;
                }
                return true;
            }

            /// <summary>
            /// 캐릭터 머리 위에 뜨는 말풍선 넓이 계산 함수.
            /// 기존 함수는 영문자 기준이라서 한글 기준으로 바꿈.
            /// </summary>
            /// <param name="text">원본 문자열</param>
            /// <param name="symWidth">폰트 크기(넓이 아님)</param>
            /// <returns>말풍선 넓이 px</returns>
            private static float MyCalculateBarkWidth(string text, float symWidth)
            {
                int length = text.Length;

                if (length > 25)
                {
                    string[] words = text.Split(' ');
                    int num = 0;
                    foreach (string word in words)
                    {
                        num += word.Length;
                        if (num > 20)
                        {
                            break;
                        }
                        num++;
                    }
                    float width = symWidth * 0.58f * num;
                    return Mathf.Max(Mathf.Ceil(symWidth * length / Mathf.Sqrt(0.625f * length)), width);
                }
                else if (length < 10)
                {
                    return Mathf.Ceil(symWidth * length);
                }
                else
                {
                    return Mathf.Ceil(symWidth * 0.8f * length);
                }
            }
        }
    }
}
