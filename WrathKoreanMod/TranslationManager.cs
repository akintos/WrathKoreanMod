using Kingmaker.Localization;
using Kingmaker.Utility;
using System.Collections;
using System.Reflection;
using WrathKoreanMod.Patch;

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
        Translation = LoadLatestTranslation(translationPath);

        string machineTranslationPath = Path.Combine(resourcesPath, "machinetranslation.json");
        if (File.Exists(machineTranslationPath))
        {
            MachineTranslation = LoadTranslationFile(machineTranslationPath);
        }

        ModMain.LogInfo("번역 데이터를 불러왔습니다.");
        Initialized = true;
    }

    private TranslationStorage LoadLatestTranslation(string translationPath)
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

        GithubClient.Release latestRelease = ghc.GetLatestRelease();
        DateTime lastReleaseTime = latestRelease.published_at.ToLocalTime();
        ModMain.LogInfo($"최신 번역 데이터 생성 시점: {lastReleaseTime}");

        if (localFileTime >= lastReleaseTime)
        {
            TranslationBuildTimestamp = localFileTime;
            ModMain.LogInfo("이미 최신 번역 데이터를 가지고 있습니다.");
        }
        else
        {
            ModMain.LogInfo("최신 번역 데이터를 다운로드합니다...");
            GithubClient.Release.Asset translationJsonAsset = latestRelease.assets.First(x => x.name == "translation.json.gz");
            ghc.DownloadGzipCompressedAssetFile(translationJsonAsset, translationPath);
            File.SetLastWriteTimeUtc(translationPath, lastReleaseTime);

            TranslationBuildTimestamp = lastReleaseTime;
            ModMain.LogInfo("다운로드가 완료되었습니다.");
        }

        return LoadTranslationFile(translationPath);
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
}
