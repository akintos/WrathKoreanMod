using Newtonsoft.Json;


namespace WrathKoreanMod;

internal class ModSettings
{
    private readonly string settingsFilePath;

    private readonly InternalSettings internalSettings;

    public ModSettings(string settingsFilePath)
    {
        this.settingsFilePath = settingsFilePath;

        if (File.Exists(settingsFilePath))
        {
            string json = File.ReadAllText(settingsFilePath);
            internalSettings = JsonConvert.DeserializeObject<InternalSettings>(json);
        }
        else
        {
            internalSettings = new InternalSettings();
            SaveSettings();
        }
    }

    public void SaveSettings()
    {
        string json = JsonConvert.SerializeObject(internalSettings, Formatting.Indented);
        File.WriteAllText(settingsFilePath, json);
    }

    public bool ShowDialogWeblateLink
    {
        get => internalSettings.ShowDialogWeblateLink;
        set
        {
            if (internalSettings.ShowDialogWeblateLink != value)
            {
                internalSettings.ShowDialogWeblateLink = value;
                SaveSettings();
            }
        }
    }

    public bool UseMachineTranslation
    {
        get => internalSettings.UseMachineTranslation;
        set
        {
            if (internalSettings.UseMachineTranslation != value)
            {
                internalSettings.UseMachineTranslation = value;
                SaveSettings();
            }
        }
    }

    public bool MachineTranslationPrefixed
    {
        get => internalSettings.MachineTranslationPrefixed;
        set
        {
            if (internalSettings.MachineTranslationPrefixed != value)
            {
                internalSettings.MachineTranslationPrefixed = value;
                SaveSettings();
            }
        }
    }

    private class InternalSettings
    {
        public bool ShowDialogWeblateLink { get; set; } = false;

        public bool UseMachineTranslation { get; set; } = false;

        public bool MachineTranslationPrefixed { get; set; } = false;
    }
}
