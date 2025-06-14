namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using Verse;

    [HarmonyPatch(typeof(HediffSet), nameof(HediffSet.DirtyCache))]
    public static class HediffSet_DirtyCache_Patch
    {
        public static void Postfix(HediffSet __instance)
        {
            __instance.pawn.RecheckPaths();
        }
    }
}