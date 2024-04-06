using System.Collections;
using System.Reflection;

using Kingmaker.Localization;
using Kingmaker.Utility;

namespace WrathKoreanMod;

internal class TranslationManager
{
    private static TranslationManager _instance;
    internal static TranslationManager Instance => _instance ??= new TranslationManager();

    private TranslationManager() { }

    public bool Initialized { get; private set; } = false;

    internal TranslationStorage Translation { get; private set; }
    internal TranslationStorage MachineTranslation { get; private set; }

    internal DateTime TranslationBuildTimestamp { get; private set; }

    public void Initialize(string resourcesPath)
    {
        if (Initialized)
        {
            return;
        }

        string translationPath = Path.Combine(resourcesPath, "translation.json");
        try
        {
            DownloadLatestTranslation(translationPath);
        }
        catch (Exception e)
        {
            ModMain.LogInfo("최신 번역 데이터를 다운로드하는데 실패했습니다. 저장된 번역 데이터를 사용합니다.");
            ModMain.LogError(e);
        }

        try
        {
            Translation = LoadTranslationFile(translationPath);
        }
        catch (Exception)
        {
            ModMain.LogInfo("저장된 번역 데이터를 불러오는데 실패했습니다. 한국어 번역을 비활성화합니다.");
            throw;
        }

        string machineTranslationPath = Path.Combine(resourcesPath, "machinetranslation.json");
        if (File.Exists(machineTranslationPath))
        {
            MachineTranslation = LoadTranslationFile(machineTranslationPath);
        }

        ModMain.LogInfo("번역 데이터를 불러왔습니다.");
        Initialized = true;
    }

    private void DownloadLatestTranslation(string translationPath)
    {
        using GithubClient ghc = new();

        DateTime localFileTime;

        if (File.Exists(translationPath))
        {
            localFileTime = File.GetLastWriteTime(translationPath);
            ModMain.LogInfo($"현재 번역 데이터 생성 시점: {localFileTime}");
        }
        else
        {
            localFileTime = DateTime.MinValue;
            ModMain.LogInfo("저장된 번역 데이터가 존재하지 않습니다.");
        }

        GithubClient.Release latestRelease;

        try
        {
            latestRelease = ghc.GetLatestRelease();
        }
        catch (Exception e)
        {
            ModMain.LogInfo("서버에서 최신 번역 정보를 가져오는데 실패했습니다.");
            ModMain.LogError(e);
            return;
        }

        DateTime lastReleaseTime = latestRelease.published_at.ToLocalTime();
        ModMain.LogInfo($"최신 번역 데이터 생성 시점: {lastReleaseTime}");

        if (localFileTime.AddMinutes(5) >= lastReleaseTime)
        {
            TranslationBuildTimestamp = localFileTime;
            ModMain.LogInfo("이미 최신 번역 데이터를 가지고 있습니다.");
            return;
        }

        ModMain.LogInfo("최신 번역 데이터를 다운로드합니다...");
        GithubClient.Release.Asset translationJsonAsset = latestRelease.assets.First(x => x.name == "translation.json.gz");

        try
        {
            ghc.DownloadGzipCompressedAssetFile(translationJsonAsset, translationPath);
        }
        catch (Exception e)
        {
            ModMain.LogInfo("번역 데이터 다운로드에 실패했습니다.");
            ModMain.LogError(e);
            return;
        }

        File.SetLastWriteTimeUtc(translationPath, lastReleaseTime);

        TranslationBuildTimestamp = lastReleaseTime;
        ModMain.LogInfo("다운로드가 완료되었습니다.");
    }

    private static TranslationStorage LoadTranslationFile(string translationPath)
    {
        var translation = NewtonsoftJsonHelper.DeserializeFromFile<TranslationStorage>(translationPath);

        ModMain.LogDebug($"{Path.GetFileName(translationPath)}: {translation.Total}");

        return translation;
    }

    public bool TryTranslate(string key, out string translated)
    {
        if (Translation is not null 
            && Translation.TryGetValue(key, out translated))
        {
            return true;
        }

        if (ModMain.Settings.UseMachineTranslation 
            && MachineTranslation is not null
            && MachineTranslation.TryGetValue(key, out translated))
        {
            if (ModMain.Settings.MachineTranslationPrefixed)
            {
                translated = $"[MT] {translated}";
            }

            return true;
        }

        translated = null;
        return false;
    }

    private static IDictionary GetOriginalLocalizationPackData()
    {
        return (IDictionary)AccessTools.Field(typeof(LocalizationPack), "m_Strings").GetValue(LocalizationManager.CurrentPack);
    }

    private static IDictionary originalLocalizationPackData;

    private static readonly Type stringEntryType = typeof(LocalizationPack).GetNestedType("StringEntry", BindingFlags.NonPublic);
    private static readonly FieldInfo stringEntryTextField = stringEntryType.GetField("Text");

    public static string GetOriginalString(string key)
    {
        originalLocalizationPackData ??= GetOriginalLocalizationPackData();
        object entry = originalLocalizationPackData[key];
        return (string)stringEntryTextField.GetValue(entry);
    }
}
