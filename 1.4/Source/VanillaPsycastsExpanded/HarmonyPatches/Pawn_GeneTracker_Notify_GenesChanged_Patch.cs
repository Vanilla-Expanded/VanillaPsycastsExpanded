namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;

    [HarmonyPatch(typeof(Pawn_GeneTracker), "Notify_GenesChanged")]
    public static class Pawn_GeneTracker_Notify_GenesChanged_Patch
    {
        public static void Postfix(Pawn_GeneTracker __instance)
        {
            __instance.pawn.RecheckPaths();
        }
    }
}