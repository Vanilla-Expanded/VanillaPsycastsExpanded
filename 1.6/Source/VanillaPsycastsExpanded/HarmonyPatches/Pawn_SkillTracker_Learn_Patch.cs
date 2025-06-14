namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [HarmonyPatch(typeof(Pawn_SkillTracker), "Learn")]
    public class Pawn_SkillTracker_Learn_Patch
    {
        private static bool Prefix(SkillDef sDef, float xp, Pawn ___pawn)
        {
            if ((___pawn.story?.traits?.HasTrait(VPE_DefOf.VPE_Thrall) ?? false) && xp > 0)
            {
                return false;
            }
            return true;
        }
    }
}