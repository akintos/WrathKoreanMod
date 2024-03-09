using Kingmaker.Utility;

namespace WrathKoreanMod;

internal class TranslationManager
{
    private static TranslationManager _instance;
    internal static TranslationManager Instance => _instance ??= new TranslationManager();

    private TranslationManager() { }

    public bool Initialized { get; private set; } = false;

    private Dictionary<string, string> Translation { get; set; }
    private Dictionary<string, string> MachineTranslation { get; set; }

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

    private Dictionary<string, string> LoadLatestTranslation(string translationPath)
    {
        using GithubClient ghc = new();

        DateTime localFileTime = File.GetLastWriteTime(translationPath);
        ModMain.LogInfo($"현재 번역 데이터 생성 시점: {localFileTime}");

        GithubClient.Release latestRelease = ghc.GetLatestRelease();
        DateTime lastReleaseTime = latestRelease.published_at.ToLocalTime();
        ModMain.LogInfo($"최신 번역 데이터 생성 시점: {lastReleaseTime}");

        if (localFileTime >= lastReleaseTime)
        {
            ModMain.LogInfo("이미 최신 번역 데이터를 가지고 있습니다.");
            return LoadTranslationFile(translationPath);
        }

        ModMain.LogInfo("최신 번역 데이터를 다운로드합니다...");
        GithubClient.Release.Asset translationJsonAsset = latestRelease.assets.First(x => x.name == "translation.json");
        ghc.DownloadAssetFile(translationJsonAsset, translationPath);
        File.SetLastWriteTimeUtc(translationPath, lastReleaseTime);

        return LoadTranslationFile(translationPath);
    }

    private static Dictionary<string, string> LoadTranslationFile(string translationPath)
    {
        Dictionary<string, string> translation;

        using (StreamReader streamReader = new(translationPath))
        {
            translation = NewtonsoftJsonHelper.Deserialize<Dictionary<string, string>>(streamReader);
        }

        ModMain.LogDebug($"{Path.GetFileName(translationPath)}: {translation.Count}");

        return translation;
    }

    public bool TryTranslate(string key, out string translated)
    {
        if (Translation is not null && Translation.TryGetValue(key, out translated))
        {
            return true;
        }

        if (ModMain.Settings.UseMachineTranslation 
            && MachineTranslation != null
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
