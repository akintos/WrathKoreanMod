using Kingmaker.Localization.Shared;
using Kingmaker.Localization;
namespace WrathKoreanMod.Patch;

/// <summary>
/// 메인 번역 훅
/// </summary>
[HarmonyPatch(typeof(LocalizationPack), nameof(LocalizationPack.GetText))]
internal class LocalizationPack_GetText_Hook
{
    public static bool Prefix(string key, LocalizationPack __instance, ref string __result)
    {
        if (ModMain.Enabled
            && __instance.Locale != Locale.Sound
            && TranslationManager.Instance.Initialized
            && TranslationManager.Instance.TryTranslate(key, out string tr))
        {
            __result = tr;
            return false; // skip original GetText
        }

        return true; // fallback to original GetText
    }
}
