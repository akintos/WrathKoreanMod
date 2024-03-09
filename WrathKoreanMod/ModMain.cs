using System.Diagnostics;
using System.Reflection;

using UnityModManagerNet;

using TMPro;

using Kingmaker;
using Kingmaker.TextTools;
using WrathKoreanMod.TextTemplates;

namespace WrathKoreanMod;

public class ModMain
{
    internal static UnityModManager.ModEntry ModEntry = null;
    internal static string ModDirectory { get; private set; }
    internal static bool Enabled { get; private set; }

    private static TMP_FontAsset KoreanFont;

    internal static ModSettings Settings { get; private set; }

    internal static bool Load(UnityModManager.ModEntry modEntry)
    {
        ModEntry = modEntry;
        ModDirectory = modEntry.Path;
        Logger = modEntry.Logger;
        Enabled = modEntry.Enabled;

        try
        {
            string settingsPath = Path.Combine(ModEntry.Path, "settings.json");
            Settings = new ModSettings(settingsPath);

            RegisterTextTemplates();

            Harmony harmony = new(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        catch (Exception ex)
        {
            LogError(ex);
            LogError(ex.StackTrace);
            throw;
        }

        modEntry.OnGUI = OnGui;
        modEntry.OnToggle = OnToggle;
        // modEntry.OnUpdate = OnUpdate;

        return true;
    }

    private delegate void AddTemplateDelegate(string tag, TextTemplate template);

    private static void RegisterTextTemplates()
    {
        MethodInfo method = AccessTools.Method(typeof(TextTemplateEngine), "AddTemplate");
        AddTemplateDelegate addTemplateDelegate = AccessTools.MethodDelegate<AddTemplateDelegate>(method);

        addTemplateDelegate("is_commander", new PlayerIsCommanderTemplate());
    }

    static void OnGui(UnityModManager.ModEntry modEntry)
    {
        Settings.ShowDialogWeblateLink = GUILayout.Toggle(Settings.ShowDialogWeblateLink, "대사에 웹레이트 링크 표시");
        Settings.UseMachineTranslation = GUILayout.Toggle(Settings.UseMachineTranslation, "기계번역 사용");
        
        if (Settings.UseMachineTranslation)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Settings.MachineTranslationPrefixed = GUILayout.Toggle(Settings.MachineTranslationPrefixed, "기계번역 앞에 [MT] 표시");
            GUILayout.EndHorizontal();
        }
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

    [HarmonyPatch(typeof(Kingmaker.Localization.LocalizationManager), "Init")]
    private class LocalizationManager_Init_Hook
    {
        public static void Postfix()
        {
            try
            {
                string resourcesPath = Path.Combine(ModEntry.Path, "Resources");
                TranslationManager.Instance.Initialize(resourcesPath);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }
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
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }
    }

    #region logging functions
    [Conditional("DEBUG")]
    internal static void LogDebug(string msg) => Logger?.Log(msg);
    internal static void LogError(Exception ex) => Logger?.Error(ex.ToString());
    internal static void LogError(string msg) => Logger?.Error(msg);
    internal static void LogInfo(string msg) => Logger?.Log(msg);
    internal static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
    #endregion
}
