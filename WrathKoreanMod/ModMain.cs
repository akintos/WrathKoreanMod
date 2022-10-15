using System.Diagnostics;
using System.Reflection;

using UnityModManagerNet;

using TMPro;

using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Kingmaker.EntitySystem.Persistence.JsonUtility;

using Newtonsoft.Json;
using Kingmaker;
using Kingmaker.Localization.Shared;

namespace WrathKoreanMod;

public class ModMain
{
    private static UnityModManager.ModEntry ModEntry = null;
    public static bool Enabled { get; private set; }

    private static Dictionary<string, string> translation = null;

    private static TMP_FontAsset KoreanFont;

    internal static ModSettings Settings { get; private set; }

    internal static bool Load(UnityModManager.ModEntry modEntry)
    {
        ModEntry = modEntry;
        Logger = modEntry.Logger;
        Enabled = modEntry.Enabled;

        try
        {
            var settingsPath = Path.Combine(ModEntry.Path, "settings.json");
            Settings = new ModSettings(settingsPath);

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        catch (Exception ex)
        {
            Error(ex);
            throw;
        }

        modEntry.OnGUI = OnGui;
        modEntry.OnToggle = OnToggle;
        // modEntry.OnUpdate = OnUpdate;

        return true;
    }

    static void OnGui(UnityModManager.ModEntry modEntry)
    {
        Settings.ShowDialogWeblateLink = GUILayout.Toggle(Settings.ShowDialogWeblateLink, "대사에 웹레이트 링크 표시");
    }

    static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
    {
        Enabled = value;
        return true;
    }

    private static void LoadFont()
    {
        string bundlePath = Path.Combine(ModEntry.Path, "Resources", "sourcehanserifk");
        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        KoreanFont = bundle.LoadAsset<TMP_FontAsset>("SourceHanSerifK-SemiBold SDF");

        var faceInfo = KoreanFont.faceInfo;
        faceInfo.ascentLine = 63;
        KoreanFont.faceInfo = faceInfo;
        KoreanFont.italicStyle = 0;

        LogDebug("Loaded font bundle!");

        TMP_FontAsset nexusSerif = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(x => x.name == "NexusSerif-Regular SDF");

        nexusSerif.fallbackFontAssetTable.Add(KoreanFont);
        LogDebug($"Patched {nexusSerif.name} with {KoreanFont}");
    }

    private static void LoadTranslation()
    {
        var translationPath = Path.Combine(ModEntry.Path, "Resources", "translation.json");

        JsonSerializer jsonSerializer = JsonSerializer.Create(DefaultJsonSettings.DefaultSettings);

        using (StreamReader streamReader = new StreamReader(translationPath))
        using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
        {
            translation = jsonSerializer.Deserialize<Dictionary<string, string>>(jsonTextReader);
        }
        LogDebug($"Loaded translation - {translation.Count} strings");
    }

    /// <summary>
    /// GameStarter.FixTMPAssets 서브루틴에서 모든 폰트를 초기화시킴. 해당 함수 뒤에 폰트 다시 로드.
    /// </summary>
    [HarmonyPatch(typeof(GameStarter), "FixTMPAssets")]
    private class GameStarter_FixTMPAssets_Hook
    {
        public static void Postfix()
        {
            if (KoreanFont != null)
            {
                return;
            }

            try
            {
                LoadFont();
                LoadTranslation();
            }
            catch (Exception e)
            {
                Error(e);
            }
        }
    }

    /// <summary>
    /// 메인 번역 훅
    /// </summary>
    [HarmonyPatch(typeof(LocalizationPack), "GetText")]
    private class LocalizationPack_GetText_Hook
    {
        public static bool Prefix(string key, LocalizationPack __instance, ref string __result)
        {
            if (Enabled && __instance.Locale != Locale.Sound && translation != null && translation.TryGetValue(key, out string tr))
            {
                __result = tr;
                return false; // skip original GetText
            }

            return true; // fallback to original GetText
        }
    }

    #region logging functions
    [Conditional("DEBUG")]
    internal static void LogDebug(string msg) => Logger.Log(msg);
    internal static void Error(Exception ex) => Logger?.Error(ex.ToString());
    internal static void Error(string msg) => Logger?.Error(msg);
    internal static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
    #endregion
}
