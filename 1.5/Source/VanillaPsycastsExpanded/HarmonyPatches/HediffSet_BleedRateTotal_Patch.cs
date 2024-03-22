namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using Verse;
    using VFECore;

    [HarmonyPatch(typeof(HediffSet), nameof(HediffSet.BleedRateTotal), MethodType.Getter)]
    public static class HediffSet_BleedRateTotal_Patch
    {
        public static void Postfix(ref float __result, HediffSet __instance)
        {
            if (__result > 0)
            {
                if (__instance?.GetFirstHediffOfDef(VPE_DefOf.VPE_BlockBleeding) != null)
                {
                    __result = 0;
                }
            }
        }
    }
}