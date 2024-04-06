using Kingmaker.UI.MVVM._VM.MainMenu;

namespace WrathKoreanMod.Patch;

internal static class IntroductoryTextPatch
{
    const string RPG_GALLERY_URL = "https://gall.dcinside.com/mgallery/board/lists?id=rpgundivded";

    public static string KoreanText { get; set; } = "패스파인더: 의인의 분노 인핸스드 에디션과 함께 " +
                "골라리온으로 돌아가 신화적인 모험을 떠나세요.\n" +
                "한국어 패치를 이용해 주셔서 감사합니다. 즐거운 게임 되시길!\n\n" +
                "<color=#3e004b><u><link=\"" + RPG_GALLERY_URL + "\">RPG 마이너 갤러리</link></u></color>" +
                "에서 다른 플레이어들과 이야기에 참여해 보세요.";

    //public static void SetTranslationProgressText(int translated, int total, DateTime updateTime)
    //{
    //    float percent = (float)translated / total * 100;
    //    KoreanText = 
    //        $"패스파인더: 의인의 분노 번역은 현재 {translated} / {total} ({percent:0.00}%) 진행되었습니다.\n" +
    //        $"마지막 번역 업데이트는 {updateTime:yyyy년 M월 d일 H시 m분} 입니다.";
    //}

    [HarmonyPatch(typeof(MainMenuVM), nameof(MainMenuVM.GetIntroductoryText))]
    private static class MainMenuVM_GetIntroductoryText_Patch
    {
        public static bool Prefix(ref string __result)
        {
            __result = KoreanText;
            return false;
        }
    }
}
