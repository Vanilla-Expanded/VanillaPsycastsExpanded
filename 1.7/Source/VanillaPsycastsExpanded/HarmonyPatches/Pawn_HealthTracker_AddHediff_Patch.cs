namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;
    using System;
    using Verse;

    [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", new Type[]
	{
		typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult)
	})]
	public static class Pawn_HealthTracker_AddHediff_Patch
	{
		public static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn, Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
		{
			if (hediff.def == HediffDefOf.Hypothermia && ___pawn.health.hediffSet.HasHediff(VPE_DefOf.VPE_IceShield))
			{
				return false;
			}
			return true;
		}
	}
}