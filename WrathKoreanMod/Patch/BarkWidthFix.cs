using Kingmaker.UI.Common;

namespace WrathKoreanMod.Patch;

[HarmonyPatch(typeof(UIUtility), nameof(UIUtility.CalculateBarkWidth))]
internal static class UIUtility_CalculateBarkWidth_Patch
{
    public static void Postfix(string text, float symWidth, ref float __result)
    {
        if (ModMain.Enabled)
        {
            __result = MyCalculateBarkWidth(text, symWidth);
        }
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
            float width1 = symWidth * 0.58f * num;
            float width2 = Mathf.Ceil(symWidth * length / Mathf.Sqrt(0.625f * length));
            return Mathf.Max(width1, width2);
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

/* 원본 디컴파일 코드
public static float CalculateBarkWidth(string text, float symWidth)
{
    int length = text.Length;
    float num = 0f;
    if (text.Length > 25)
    {
        string[] array = text.Split(' ');
        int num2 = 0;
        string[] array2 = array;
        foreach (string text2 in array2)
        {
            num2 += text2.Length;
            if (num2 > 20)
            {
                break;
            }

            num2++;
        }

        num = symWidth * 0.58f * (float)num2;
        return Mathf.Max(Mathf.Ceil(symWidth * (float)length / Mathf.Sqrt(0.625f * (float)length)), num);
    }

    return Mathf.Ceil(symWidth * 0.58f * (float)length);
}
*/