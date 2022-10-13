using HarmonyLib;
using Kingmaker.UI.MVVM._VM.MainMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrathKoreanMod.Patch
{
    internal static class IntroductoryText
    {
        [HarmonyPatch(typeof(MainMenuVM), "GetIntroductoryText")]
        private static class MainMenuVM_GetIntroductoryText_Patch
        {
            public static bool Prefix(ref string __result)
            {
                __result = "패스파인더: 의인의 분노 인핸스드 에디션과 함께 " +
                    "골라리온으로 돌아가 신화적인 모험을 떠나세요. " +
                    "지원해 주셔서 감사합니다, 즐거운 게임 되시길!\n\n" +
                    "저희 <color=#3e004b><u><link=\"https://discord.gg/owlcat\">디스코드 서버</link></u></color>" +
                    "에 참여해 다른 패스파인더들과 게임에 대해 대화하세요.\n\n언제나 감사드립니다,\n아울캣 게임즈";

                return false;
            }
        }
    }
}
