namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [HarmonyPatch(typeof(ThoughtWorker_Cold), "CurrentStateInternal")]
    public static class ThoughtWorker_Cold_CurrentStateInternal
    {
        public static void Postfix(Pawn p, ref ThoughtState __result)
        {
            if (p.health.hediffSet.HasHediff(VPE_DefOf.VPE_IceShield))
            {
                __result = false;
            }
        }
    }
}