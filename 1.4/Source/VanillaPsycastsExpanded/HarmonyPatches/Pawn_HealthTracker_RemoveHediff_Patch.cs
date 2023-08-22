namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using Verse;

    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RemoveHediff))]
    public static class Pawn_HealthTracker_RemoveHediff_Patch
    {
        public static bool Prefix(Pawn_HealthTracker __instance, Hediff hediff)
        {
            if (hediff.def == VPE_DefOf.VPE_Sacrificed)
            {
                return false;
            }
            return true;
        }
    }
}