using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
public class Pawn_NeedsTracker_ShouldHaveNeed_Patch
{
    private static bool Prefix(NeedDef nd, Pawn ___pawn)
    {
        try
        {
            if ((nd == NeedDefOf.Rest || nd == VPE_DefOf.Joy) && (___pawn.story?.traits?.HasTrait(VPE_DefOf.VPE_Thrall) ?? false)) return false;
        }
        catch
        {

        }
        return true;
    }
}
