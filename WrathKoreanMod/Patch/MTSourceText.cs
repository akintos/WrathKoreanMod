using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.TextTools;
using Kingmaker.UI.MVVM._VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM._VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;

namespace WrathKoreanMod.Patch;

internal class MTSourceText
{
    /// <summary>
    /// 대사 텍스트 앞에 웹레이트 링크 태그 추가하는 패치
    /// </summary>
    [HarmonyPatch(typeof(BlueprintCue), nameof(BlueprintCue.DisplayText), MethodType.Getter)]
    private static class BlueprintCue_GetDisplayText_Patch
    {
        public static void Postfix(BlueprintCue __instance, ref string __result)
        {
            if (__result.Contains("[MT]"))
            {
                string textKey = __instance.Text.Key;
                __result = __result.Replace("[MT]", $"<link=\"Source:{textKey}\">[MT]</link>");
            }
        }
    }

    /// <summary>
    /// 플레이어 선택지 텍스트 앞에 웹레이트 링크 태그 추가하는 패치
    /// </summary>
    [HarmonyPatch(typeof(BlueprintAnswer), nameof(BlueprintAnswer.DisplayText), MethodType.Getter)]
    private static class BlueprintAnswer_GetDisplayText_Patch
    {
        public static void Postfix(BlueprintAnswer __instance, ref string __result)
        {
            if (__result.Contains("[MT]"))
            {
                string textKey = __instance.Text.Key;
                __result = __result.Replace("[MT]", $"<link=\"Source:{textKey}\">[MT]</link>");
            }
        }
    }

    /// <summary>
    /// 툴팁 데이터를 불러올때 웹레이트 링크인지 구분하기 위한 패치
    /// </summary>
    [HarmonyPatch(typeof(TooltipHelper))]
    [HarmonyPatch(nameof(TooltipHelper.GetLinkTooltipTemplate),
        typeof(string[]),
        typeof(List<SkillCheckDC>),
        typeof(List<SkillCheckResult>)
    )]
    private static class TooltipHelper_GetLinkTooltipTemplate_Patch
    {
        public static bool Prefix(string[] keys, ref TooltipBaseTemplate __result)
        {
            if (keys[0] == "Source")
            {
                __result = new TooltipTemplateSourceText(keys[1]);
                return false;
            }
            return true;
        }
    }

    private class TooltipTemplateSourceText : TooltipBaseTemplate
    {
        public string DialogStringKey { get; private set; }
        public string SourceTextRaw { get; private set; }
        public string SourceTextFormatted { get; private set; }

        public TooltipTemplateSourceText(string dialogStringKey)
        {
            DialogStringKey = dialogStringKey;

            SourceTextRaw = TranslationManager.GetOriginalString(dialogStringKey);
            SourceTextFormatted = TextTemplateEngine.Process(SourceTextRaw);
        }

        public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
        {
            if (type == TooltipTemplateType.Info)
            {
                yield return new TooltipBrickTitle("기계번역 원문");
            }
            yield break;
        }

        public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
    {
            if (type == TooltipTemplateType.Info)
            {
                yield return new TooltipBrickText(DialogStringKey, TooltipTextType.BoldCentered);
            }
            yield return new TooltipBrickText(SourceTextFormatted);
            yield break;
        }

        public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
        {
            if (type == TooltipTemplateType.Info)
            {
                yield return new TooltipBrickButton(CopyStringKeyToClipboard, "키 복사");
                yield return new TooltipBrickButton(CopySourceTextToClipboard, "원문 복사");
            }
            yield break;
        }

        public override IEnumerable<ITooltipBrick> GetHint(TooltipTemplateType type)
        {
            yield return new TooltipBrickText("키 복사: 클립보드에 키를 복사합니다.");
            yield break;
        }

        public void CopyStringKeyToClipboard()
        {
            GUIUtility.systemCopyBuffer = DialogStringKey;
        }

        public void CopySourceTextToClipboard()
        {
            GUIUtility.systemCopyBuffer = SourceTextRaw;
        }
    }
}
