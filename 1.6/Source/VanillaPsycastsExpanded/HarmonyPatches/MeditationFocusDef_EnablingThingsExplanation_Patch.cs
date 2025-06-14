namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [HarmonyPatch(typeof(MeditationFocusDef), nameof(MeditationFocusDef.EnablingThingsExplanation))]
    public static class MeditationFocusDef_EnablingThingsExplanation_Patch
    {
        public static void Postfix(Pawn pawn, MeditationFocusDef __instance, ref string __result)
        {
            if (pawn.Psycasts()?.unlockedMeditationFoci.Contains(__instance) ?? false)
                __result += "\n  - " + "VPE.UnlockedByPoints".Translate() + ".";
        }
    }
}