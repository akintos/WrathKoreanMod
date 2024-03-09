using System.Reflection.Emit;
using System.Reflection;

namespace WrathKoreanMod.ModSupport;

[HarmonyPatch]
internal static class ScalingCantripsPatch
{
    public static bool Prepare(MethodBase original)
    {
        if (original is not null)
        {
            return true;
        }

        targetMethod = AccessTools.Method("ScalingCantrips.Utilities.Helpers:CreateString");
        ModMain.LogInfo($"ScalingCantrips 설치: {targetMethod != null}");
        return targetMethod != null;
    }

    public static T GetSetting<T>(string name) where T : struct
    {
        object settingsobj = AccessTools
            .TypeByName("ScalingCantrips.Main")
            .GetField("settings", BindingFlags.Static | BindingFlags.Public)
            .GetValue(null);

        return (T)settingsobj.GetType().GetField(name).GetValue(settingsobj);
    }

    public static MethodBase TargetMethod()
    {
        return targetMethod;
    }

    public static void Prefix(string key, ref string value)
    {
        switch (key)
        {
            case "RMAddWisStatToDamage.Name":
                value = "캔트립 전문가 (지혜)";
                break;
            case "RMAddWisStatToDamage.Description":
                value = "당신이 시전하는 캔트립이 {g|Encyclopedia:Wisdom}지혜{/g} 능력치 보너스 만큼의 피해 보너스를 얻습니다.";
                break;
            case "RMAddIntStatToDamage.Name":
                value = "캔트립 전문가 (지능)";
                break;
            case "RMAddIntStatToDamage.Description":
                value = "당신이 시전하는 캔트립이 {g|Encyclopedia:Intellegence}지능{/g} 능력치 보너스 만큼의 피해 보너스를 얻습니다.";
                break;
            case "RMAddChaStatToDamage.Name":
                value = "캔트립 전문가 (매력)";
                break;
            case "RMAddChaStatToDamage.Description":
                value = "당신이 시전하는 캔트립이 {g|Encyclopedia:Charisma}매력{/g} 능력치 보너스 만큼의 피해 보너스를 얻습니다.";
                break;
            case "RMFirebolt.Name":
                value = "불꽃 화살";
                break;
            case "RMFirebolt.Description":
                value = "불의 화살을 날려 원거리 접촉 공격을 가합니다. " +
                    "대상은 {g|Encyclopedia:Dice}1d3{/g}점의 화염 " +
                    "{g|Encyclopedia:Damage}피해{/g}를 입습니다. 시전자 레벨 " + 
                    GetSetting<int>("CasterLevelsReq") + 
                    " 마다 피해 굴림 주사위가 1개 추가되며, 피해량이 최대 " + 
                    GetSetting<int>("MaxDice") + "d3 까지 증가합니다.";
                break;
            case "RMUnholyZap.Name":
            case "RMUnholyZapEffect.Name":
                value = "불경한 충격";
                break;
            case "RMUnholyZap.Description":
            case "RMUnholyZapEffect.Description":
                value = "단일 대상에게 불경한 힘을 해방합니다. " +
                    "대상은 {g|Encyclopedia:Dice}1d3{/g}점의 음에너지 " +
                    "{g|Encyclopedia:Damage}피해{/g}를 입습니다. 시전자 레벨 " +
                    GetSetting<int>("CasterLevelsReq") +
                    " 마다 피해 굴림 주사위가 1개 추가되며, 피해량이 최대 " +
                    GetSetting<int>("MaxDice") +
                    "d3 까지 증가합니다. 인내 {g|Encyclopedia:Saving_Throw}내성 굴림{/g}에 성공하면 " +
                    "피해가 절반으로 감소합니다.";
                break;
            case "RMJoltingGrasp.Name":
            case "RMJoltingGraspEffect.Name":
                value = "감전의 손아귀";
                break;
            case "RMJoltingGrasp.Description":
            case "RMJoltingGraspEffect.Description":
                value = "근접 {g|Encyclopedia:TouchAttack}접촉 공격{/g}에 성공하면" +
                    GetSetting<int>("JoltingGraspLevelsReq") + 
                    " 시전자 레벨당 {g|Encyclopedia:Dice}1d3{/g} 점의 " +
                    "{g|Encyclopedia:Energy_Damage}전기 피해{/g}를 가합니다 (최대 " + 
                    GetSetting<int>("JoltingGraspMaxDice") +
                    "d6). 적을 감전시킬 때, 대상이 금속 갑옷이나 무기를 착용하고 있을 경우 " + 
                    "{g|Encyclopedia:Attack}명중 굴림{/g} +3 {g|Encyclopedia:Bonus}보너스{/g}를 받습니다.";
                break;
            default:
                break;
        }
    }

    private static MethodBase targetMethod;
}

[HarmonyPatch]
internal static class CantripPatcher_Patch
{
    public static bool Prepare(MethodBase original)
    {
        return AccessTools.TypeByName("ScalingCantrips.CantripPatcher") != null;
    }

    public static IEnumerable<MethodBase> TargetMethods()
    {
        Type cls = AccessTools.TypeByName("ScalingCantrips.CantripPatcher+BlueprintPatcher");
        yield return cls.GetMethod("EditAndAddTempHP", BindingFlags.Static | BindingFlags.NonPublic);
        yield return cls.GetMethod("EditAndAddAbility", BindingFlags.Static | BindingFlags.NonPublic);
        yield return cls.GetMethod("EditAndAddAbilityUndead", BindingFlags.Static | BindingFlags.NonPublic);
        yield break;
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction codeInstruction in instructions)
        {
            if (codeInstruction.opcode == OpCodes.Ldstr)
            {
                string text = codeInstruction.operand as string;
                if (TranslationDictionary.TryGetValue(text, out string translation))
                {
                    codeInstruction.operand = translation;
                }
            }
        }

        return instructions;
    }

    private static readonly Dictionary<string, string> TranslationDictionary = new()
    {
        { "For every ", "시전자 레벨 " },
        { " caster level(s) the caster has, Virtue will grant another point of temporary HP, up to ", " 마다, 미덕은 대상에게 1점의 추가 임시 생명점을 부여하며, 최대 " },
        { " points total.", " 점까지 증가합니다." },

        { " Damage dice is increased by 1 every ", " 피해 주사위는 시전자 레벨 " },
        { " caster level(s), up to a maximum of ", " 당 1개씩 증가하며, 최대 " },
        { "d3.", "d3 까지 증가할 수 있습니다." },

        { " Damage dice is increased by 1 at every ", " 피해 주사위는 시전자 레벨 " },
        { " caster level(s), up to a maximum of + ", " 당 1개씩 증가하며, 최대 + " },
        { "d6.", "d6 까지 증가할 수 있습니다." }
    };
}
