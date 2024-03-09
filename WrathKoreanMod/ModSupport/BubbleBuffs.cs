using Kingmaker.Localization.Shared;
using Newtonsoft.Json;
using System.Reflection;

namespace WrathKoreanMod.ModSupport;


[SuppressMessage("CodeQuality", "IDE0051:Unused private members")]
[HarmonyPatch]
internal class BubbleBuffs
{
    private const string TARGET_CLASS_NAME = "BubbleBuffs.Config.Language";
    private const string TARGET_METHOD_NAME = "Get";

    private static Type targetClass;

    private static Dictionary<string, string> translation;

    private static bool Prepare(MethodBase original)
    {
        if (original is not null)
        {
            return true;
        }

        targetClass = AccessTools.TypeByName(TARGET_CLASS_NAME);
        bool bubbleBuffsInstalled = targetClass is not null;
        ModMain.LogInfo("BubbleBuff 설치: " + bubbleBuffsInstalled);
        if (!bubbleBuffsInstalled)
        {
            return false;
        }

        string path = Path.Combine(ModMain.ModDirectory, "ModSupport", "BubbleBuffsTranslation.json");
        using Stream stream = File.OpenRead(path);
        translation = LoadJsonDictionary(stream);
        return true;
    }

    private static MethodBase TargetMethod()
    {
        return targetClass.GetMethod(TARGET_METHOD_NAME, BindingFlags.Static | BindingFlags.Public);
    }

    private static Dictionary<string, string> LoadJsonDictionary(Stream stream)
    {
        JsonSerializer serializer = new();
        using StreamReader sr = new(stream);
        using JsonReader jr = new JsonTextReader(sr);
        return serializer.Deserialize<Dictionary<string, string>>(jr);
    }

    private static bool Prefix(string key, Locale locale, ref string __result)
    {
        if (!ModMain.Enabled || locale != Locale.enGB || translation == null)
        {
            return true;
        }
        if (translation.TryGetValue(key, out var value))
        {
            __result = value;
            return false;
        }
        return true;
    }
}