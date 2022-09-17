namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
    public class Pawn_NeedsTracker_ShouldHaveNeed_Patch
    {
        private static bool Prefix(NeedDef nd, Pawn ___pawn)
        {
            if ((nd == NeedDefOf.Rest || nd == NeedDefOf.Joy) && (___pawn.story?.traits?.HasTrait(VPE_DefOf.VPE_Thrall) ?? false))
            {
                return false;
            }
            return true;
        }
    }
}