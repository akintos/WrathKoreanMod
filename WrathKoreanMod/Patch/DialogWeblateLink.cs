using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.UI.MVVM._VM.Tooltip;
using Kingmaker.UI.MVVM._VM.Tooltip.Utils;

using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;


namespace WrathKoreanMod.Patch;

internal static class DialogWeblateLink
{
    /// <summary>
    /// 대사 텍스트 앞에 웹레이트 링크 태그 추가하는 패치
    /// </summary>
    [HarmonyPatch(typeof(BlueprintCue), "DisplayText", MethodType.Getter)]
    private static class BlueprintCue_GetDisplayText_Patch
    {
        public static void Postfix(BlueprintCue __instance, ref string __result)
        {
            if (ModMain.Enabled && ModMain.Settings.ShowDialogWeblateLink)
            {
                string textKey = __instance.Text.Key;
                string nodeId = __instance.AssetGuid.ToString().Substring(0, 8).ToUpperInvariant();
                __result = $"<link=\"Weblate:{textKey}\">[{nodeId}]</link> " + __result;
            }
        }
    }

    /// <summary>
    /// 플레이어 선택지 텍스트 앞에 웹레이트 링크 태그 추가하는 패치
    /// </summary>
    [HarmonyPatch(typeof(BlueprintAnswer), "DisplayText", MethodType.Getter)]
    private static class BlueprintAnswer_GetDisplayText_Patch
    {
        public static void Postfix(BlueprintAnswer __instance, ref string __result)
        {
            if (ModMain.Enabled && ModMain.Settings.ShowDialogWeblateLink)
            {
                string textKey = __instance.Text.Key;
                string nodeId = __instance.AssetGuid.ToString().Substring(0, 8).ToUpperInvariant();
                __result = $"<link=\"Weblate:{textKey}\">[{nodeId}]</link> " + __result;
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
            if (keys[0] == "Weblate")
            {
                __result = new TooltipTemplateWeblateLink(keys[1]);
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// 링크 툴팁 우클릭시 다이얼로그를 여는 대신 웹레이트 링크 실행
    /// </summary>
    [HarmonyPatch(
        // class name
        typeof(TooltipHelper),
        // method name
        nameof(TooltipHelper.ShowInfo),
        // argument types
        typeof(TooltipBaseTemplate),
        typeof(ConsoleNavigationBehaviour)
    )]
    private static class TooltipHelper_ShowInfo_Patch
    {
        public static bool Prefix(TooltipBaseTemplate template)
        {
            if (template is TooltipTemplateWeblateLink link)
            {
                Application.OpenURL($"https://waldo.team/translate/pathfinder-wotr/dialogue/ko/?offset=1&q=" + link.DialogStringKey);
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// 툴팁 데이터가 웹레이트 링크면 마우스 호버링시 툴팁을 표시하지 않음
    /// </summary>
    [HarmonyPatch(
        // class name
        typeof(TooltipContextVM),
        // method name
        nameof(TooltipContextVM.HandleTooltipRequest)
    )]
    private static class TooltipContextVM_HandleTooltipRequest_Patch
    {
        public static bool Prefix(TooltipData data)
        {
            if (data?.MainTemplate is TooltipTemplateWeblateLink)
            {
                return false;
            }
            return true;
        }
    }

    private class TooltipTemplateWeblateLink : TooltipBaseTemplate
    {
        public string DialogStringKey { get; private set; }

        public TooltipTemplateWeblateLink(string dialogStringKey)
        {
            DialogStringKey = dialogStringKey;
        }
    }
}
