
namespace WrathKoreanMod.Patch;

internal static class ProcessHangulJosa
{
    /// <summary>
    /// 한글 조사 처리 훅
    /// </summary>
    [HarmonyPatch(typeof(Kingmaker.TextTools.TextTemplateEngine), "Process")]
    private class TextTemplateEngine_Process_Hook
    {
        public static void Postfix(ref string __result)
        {
            if (ModMain.Enabled)
            {
                __result = Josa.Process(__result);
            }
        }
    }
}
