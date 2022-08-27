using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using UnityModManagerNet;
using HarmonyLib;

using UnityEngine;
using TMPro;

using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Kingmaker.EntitySystem.Persistence.JsonUtility;

using Newtonsoft.Json;
using System.Text;

namespace WrathKoreanMod
{
    public class Main
    {
        [Conditional("DEBUG")]
        internal static void Log(string msg) => Logger.Log(msg);
        internal static void Error(Exception ex) => Logger?.Error(ex.ToString());
        internal static void Error(string msg) => Logger?.Error(msg);
        internal static UnityModManager.ModEntry.ModLogger Logger { get; private set; }

        private static UnityModManager.ModEntry ModEntry = null;
        private static bool Enabled;

        private static Dictionary<string, string> translation = null;

        internal static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;
            Logger = modEntry.Logger;
            Enabled = modEntry.Enabled;

            try
            {
                var harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                LoadFont();
                LoadTranslation();
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }

            modEntry.OnToggle = OnToggle;
            // modEntry.OnGUI = OnGUI;
            // modEntry.OnUpdate = OnUpdate;

            return true;
        }

        private static void LoadFont()
        {
            var bundlePath = Path.Combine(ModEntry.Path, "Resources", "sourcehanserifk");

            AssetBundle ab = AssetBundle.LoadFromFile(bundlePath);

            var krfont = ab.LoadAsset<TMP_FontAsset>("SourceHanSerifK-SemiBold SDF");

            var faceInfo = krfont.faceInfo;
            faceInfo.ascentLine = 63;
            krfont.faceInfo = faceInfo;

            Log("Loaded font bundle");

            foreach (var fontAsset in Resources.FindObjectsOfTypeAll<TMP_FontAsset>().Where(x => x.name == "NexusSerif-Regular SDF"))
            {
                fontAsset.fallbackFontAssetTable.Add(krfont);
            }
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
            Log($"Loaded translation - {translation.Count} strings");
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        [HarmonyPatch(typeof(LocalizationPack), "GetText")]
        public class LocalizationPack_GetText_Hook
        {
            public static bool Prefix(string key, ref string __result)
            {
                if (Enabled && translation != null && translation.TryGetValue(key, out string tr))
                {
                    __result = tr;
                    return false; // skip original GetText
                }

                return true; // fallback to original GetText
            }
        }


        [HarmonyPatch(typeof(Kingmaker.UI.Common.UIUtility), "CalculateBarkWidth")]
        public class UIUtility_CalculateBarkWidth_Patch
        {
            public static bool Prefix(string text, float symWidth, ref float __result)
            {
                if (Enabled)
                {
                    __result = CalculateBarkWidth(text, symWidth);
                    return false;
                }
                return true;
            }
        }

        public static float CalculateBarkWidth(string text, float symWidth)
        {
            int length = text.Length;

            if (text.Length > 25)
            {
                string[] words = text.Split(new char[] { ' ' });
                int num = 0;
                foreach (string word in words)
                {
                    num += word.Length;
                    if (num > 20)
                    {
                        break;
                    }
                    num++;
                }
                float width = symWidth * 0.58f * num;
                return Mathf.Max(Mathf.Ceil(symWidth * length / Mathf.Sqrt(0.625f * length)), width);
            }
            else if (text.Length < 10)
            {
                return Mathf.Ceil(symWidth * length);
            }
            else
            {
                return Mathf.Ceil(symWidth * 0.8f * length);
            }
        }
    }
}
