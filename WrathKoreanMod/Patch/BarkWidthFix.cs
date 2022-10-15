

namespace WrathKoreanMod.Patch;

internal class BarkWidthFix
{
    [HarmonyPatch(typeof(Kingmaker.UI.Common.UIUtility), "CalculateBarkWidth")]
    private static class UIUtility_CalculateBarkWidth_Patch
    {
        public static bool Prefix(string text, float symWidth, ref float __result)
        {
            if (ModMain.Enabled)
            {
                __result = MyCalculateBarkWidth(text, symWidth);
                return false;
            }
            return true;
        }

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
