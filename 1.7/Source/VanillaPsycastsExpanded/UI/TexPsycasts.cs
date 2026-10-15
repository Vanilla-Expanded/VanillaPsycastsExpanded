namespace VanillaPsycastsExpanded.UI
{
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    public static class TexPsycasts
    {
        public static Texture2D IconPsyfocusGain        = ContentFinder<Texture2D>.Get("UI/Icons/IconPsyfocusGain");
        public static Texture2D IconFocusTypes          = ContentFinder<Texture2D>.Get("UI/Icons/IconFocusTypes");
        public static Texture2D IconNeuralHeatLimit     = ContentFinder<Texture2D>.Get("UI/Icons/IconNeuralHeatLimit");
        public static Texture2D IconNeuralHeatRegenRate = ContentFinder<Texture2D>.Get("UI/Icons/IconNeuralHeatRegenRate");
        public static Texture2D IconPsychicSensitivity  = ContentFinder<Texture2D>.Get("UI/Icons/IconPsychicSensitivity");
        public static Texture2D IconPsyfocusCost        = ContentFinder<Texture2D>.Get("UI/Icons/IconPsyfocusCost");
    }
}