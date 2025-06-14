namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [HarmonyPatch(typeof(ThoughtWorker_PsychicDrone), "CurrentStateInternal")]
    public static class ThoughtWorker_PsychicDrone_CurrentStateInternal_Patch
    {
        public static void Postfix(Pawn p, ref ThoughtState __result)
        {
            if (__result.StageIndex != 0 && p.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_PsychicSoothe) != null)
            {
                __result = ThoughtState.ActiveAtStage(0);
            }
        }
    }
}