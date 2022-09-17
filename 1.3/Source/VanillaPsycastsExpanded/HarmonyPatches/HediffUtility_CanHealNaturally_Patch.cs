namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using Verse;

    [HarmonyPatch(typeof(HediffUtility), "CanHealNaturally")]
    public static class HediffUtility_CanHealNaturally_Patch
    {
        private static void Postfix(ref bool __result, Hediff_Injury hd)
        {
            if (__result)
            {
                __result = hd.def != VPE_DefOf.VPE_Regenerating;
            }
        }
    }
}