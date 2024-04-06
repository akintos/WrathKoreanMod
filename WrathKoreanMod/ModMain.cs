using System.Diagnostics;
using System.Reflection;

using UnityModManagerNet;

using TMPro;

using Kingmaker;
using Kingmaker.TextTools;
using WrathKoreanMod.TextTemplates;
using WrathKoreanMod.ModSupport;

namespace WrathKoreanMod;

public class ModMain
{
    internal static UnityModManager.ModEntry ModEntry = null;
    internal static string ModDirectory { get; private set; }
    internal static bool Enabled { get; private set; }

    private static TMP_FontAsset KoreanFont;

    internal static KoreanModSettings Settings { get; private set; }

    internal static bool Load(UnityModManager.ModEntry modEntry)
    {
        ModEntry = modEntry;
        ModDirectory = modEntry.Path;
        Logger = modEntry.Logger;
        Enabled = modEntry.Enabled;

        try
        {
            Settings = UnityModManager.ModSettings.Load<KoreanModSettings>(modEntry);

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
        modEntry.OnSaveGUI = OnSaveGUI;

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
        GUIStyle titleStyle = new();
        titleStyle.fontSize = 20;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        if (GUILayout.Button("기본 설정으로 초기화", GUILayout.ExpandWidth(false)))
        {
            Settings = new KoreanModSettings();
        }

        GUILayout.Space(10);

        GUILayout.Label("최신 번역 갱신 날짜: " + TranslationManager.Instance.TranslationBuildTimestamp.ToString("yyyy년 M월 d일 H시 m분"));
        GUILayout.Label($"번역 진행률: {TranslationManager.Instance.Translation.Translated} / {TranslationManager.Instance.Translation.Total}");

        GUILayout.Label("번역 설정", titleStyle);
        GUILayout.Label("번역 설정 변경은 이미 표시된 텍스트에는 적용되지 않으며, 새로운 텍스트가 표시될 때 적용됩니다.");

        Settings.ShowDialogWeblateLink = GUILayout.Toggle(Settings.ShowDialogWeblateLink, "대사에 웹레이트 링크 표시");
        Settings.UseMachineTranslation = GUILayout.Toggle(Settings.UseMachineTranslation, "기계번역 사용");
        
        if (Settings.UseMachineTranslation)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Settings.MachineTranslationPrefixed = GUILayout.Toggle(Settings.MachineTranslationPrefixed, "기계번역 텍스트 앞에 '[MT]' 표시 (우클릭시 원문 표시)");
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);

        GUILayout.Label("모드 지원", titleStyle);
        GUILayout.Label("모드 설정 변경은 게임을 재시작해야 적용됩니다.");

        Settings.ModSupportBubbleBuffs = GUILayout.Toggle(Settings.ModSupportBubbleBuffs, "BubbleBuffs (버블버프)");
        Settings.ModSupportScalingCantrips = GUILayout.Toggle(Settings.ModSupportScalingCantrips, "Scaling Cantrips (스케일링 캔트립)");

        // add trailing space
        GUILayout.Space(10);
    }

    static void OnSaveGUI(UnityModManager.ModEntry modEntry)
    {
        Settings.Save(modEntry);
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
