namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;

    [HarmonyPatch(typeof(Pawn_AbilityTracker), nameof(Pawn_AbilityTracker.Notify_TemporaryAbilitiesChanged))]
    public static class Pawn_AbilityTracker_Notify_TemporaryAbilitiesChanged_Patch
    {
        public static void Postfix(Pawn_AbilityTracker __instance)
        {
            __instance.pawn.RecheckPaths();
        }
    }
}