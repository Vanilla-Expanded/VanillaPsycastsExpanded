namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using UnityEngine;
    using Verse;

    [HarmonyPatch(typeof(HealthUtility), "GetPartConditionLabel")]
    public static class HealthUtility_GetPartConditionLabel_Patch
    {
        private static void Postfix(ref Pair<string, Color> __result, Pawn pawn, BodyPartRecord part)
        {
            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff.def == VPE_DefOf.VPE_Sacrificed && hediff.Part == part)
                {
                    __result = new Pair<string, Color>(__result.First, Color.grey);
                }
            }
        }
    }
}