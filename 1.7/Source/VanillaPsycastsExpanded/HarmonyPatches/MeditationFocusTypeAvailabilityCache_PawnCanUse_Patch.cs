namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [HarmonyPatch(typeof(MeditationFocusTypeAvailabilityCache), "PawnCanUseInt")]
    public static class MeditationFocusTypeAvailabilityCache_PawnCanUse_Patch
    {
        public static void Postfix(Pawn p, MeditationFocusDef type, ref bool __result)
        {
            if (p.Psycasts()?.unlockedMeditationFoci.Contains(type) ?? false) __result     = true;
            else if (type.GetModExtension<MeditationFocusExtension>()?.pointsOnly ?? false) __result = false;
        }
    }
}