namespace VanillaPsycastsExpanded.HarmonyPatches
{
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [HarmonyPatch(typeof(PawnUtility), "ShouldSendNotificationAbout")]
    public static class PawnUtility_ShouldSendNotificationAbout_Patch
    {
        public static void Postfix(ref bool __result, Pawn p)
        {
            if (__result && p.kindDef == VPE_DefOf.VPE_SummonedSkeleton)
            {
                __result = false;
            }
        }
    }
}