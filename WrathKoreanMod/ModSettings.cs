using Kingmaker.Blueprints.JsonSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrathKoreanMod
{
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

        private class InternalSettings
        {
            public bool ShowDialogWeblateLink { get; set; } = false;
        }
    }
}
