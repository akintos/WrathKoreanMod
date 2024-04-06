using UnityModManagerNet;

namespace WrathKoreanMod;

public class KoreanModSettings : UnityModManager.ModSettings
{
    public override void Save(UnityModManager.ModEntry modEntry) => Save(this, modEntry);

    public bool ShowDialogWeblateLink = false;

    public bool UseMachineTranslation = true;

    public bool MachineTranslationPrefixed = false;

    public bool ModSupportBubbleBuffs = true;

    public bool ModSupportScalingCantrips = true;
}
