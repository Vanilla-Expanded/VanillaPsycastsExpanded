namespace VanillaPsycastsExpanded
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using RimWorld;
    using Verse;
    using Verse.AI;

    [HarmonyPatch(typeof(RoomStatDef), nameof(RoomStatDef.GetScoreStageIndex))]
    public static class RoomStatDef_GetScoreStageIndex_Patch
    {
        public static Pawn forPawn;
        public static void Postfix(RoomStatDef __instance, ref int __result)
        {
            if (forPawn != null && forPawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_Hallucination) != null)
            {
                __result = __instance.scoreStages.Count - 1;
            }
            forPawn = null;
        }
    }

    [HarmonyPatch]
    public static class JobDriver_Reign_Patch
    {
        [HarmonyTargetMethod]
        public static MethodBase GetMethod()
        {
            return typeof(JobDriver_Reign).GetMethods(AccessTools.all).Last(x => x.Name.Contains("<MakeNewToils>"));
        }

        public static void Prefix(JobDriver_Reign __instance)
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = __instance.pawn;
        }
        public static void Postfix()
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = null;
        }
    }

    [HarmonyPatch(typeof(JoyUtility), nameof(JoyUtility.TryGainRecRoomThought))]
    public static class JoyUtility_TryGainRecRoomThought_Patch
    {
        public static void Prefix(Pawn pawn)
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = pawn;
        }
        public static void Postfix()
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = null;
        }
    }

    [HarmonyPatch(typeof(ThoughtWorker_Ascetic), "CurrentStateInternal")]
    public static class ThoughtWorker_Ascetic_CurrentStateInternal_Patch
    {
        public static void Prefix(Pawn p)
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = p;
        }
        public static void Postfix()
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = null;
        }
    }

    [HarmonyPatch(typeof(ThoughtWorker_Greedy), "CurrentStateInternal")]
    public static class ThoughtWorker_Greedy_CurrentStateInternal_Patch
    {
        public static void Prefix(Pawn p)
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = p;
        }
        public static void Postfix()
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = null;
        }
    }

    [HarmonyPatch(typeof(ThoughtWorker_HospitalPatientRoomStats), "CurrentStateInternal")]
    public static class ThoughtWorker_HospitalPatientRoomStats_CurrentStateInternal_Patch
    {
        public static void Prefix(Pawn p)
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = p;
        }
        public static void Postfix()
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = null;
        }
    }

    [HarmonyPatch(typeof(ThoughtWorker_RoomImpressiveness), "CurrentStateInternal")]
    public static class ThoughtWorker_RoomImpressiveness_CurrentStateInternal_Patch
    {
        public static void Prefix(Pawn p)
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = p;
        }
        public static void Postfix()
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = null;
        }
    }

    [HarmonyPatch]
    public static class Toils_Ingest_FinalizeIngest_Patch
    {
        [HarmonyTargetMethod]
        public static MethodBase GetMethod()
        {
            foreach (var type in typeof(Toils_Ingest).GetNestedTypes(AccessTools.all))
            {
                var method = type.GetMethods(AccessTools.all).FirstOrDefault(x => x.Name.Contains("<FinalizeIngest>"));
                if (method != null)
                {
                    return method;
                }
            }
            throw new System.Exception("Toils_Ingest_FinalizeIngest_Patch failed to find a method to patch.");
        }

        public static void Prefix(object __instance)
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = Traverse.Create(__instance).Field("ingester").GetValue<Pawn>();
        }
        public static void Postfix()
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = null;
        }
    }


    [HarmonyPatch(typeof(Toils_LayDown), "ApplyBedThoughts")]
    public static class Toils_LayDown_ApplyBedThoughts_Patch
    {
        public static void Prefix(Pawn actor)
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = actor;
        }
        public static void Postfix()
        {
            RoomStatDef_GetScoreStageIndex_Patch.forPawn = null;
        }
    }
}