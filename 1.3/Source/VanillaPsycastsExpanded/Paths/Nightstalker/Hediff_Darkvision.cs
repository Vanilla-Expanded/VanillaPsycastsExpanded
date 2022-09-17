namespace VanillaPsycastsExpanded.Nightstalker
{
    using System.Collections.Generic;
    using System.Linq;
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [HarmonyPatch]
    public class Hediff_Darkvision : HediffWithComps
    {
        public static HashSet<Pawn> DarkvisionPawns = new();

        [HarmonyPatch(typeof(ThoughtUtility), nameof(ThoughtUtility.NullifyingHediff))]
        [HarmonyPostfix]
        public static void NullDarkness(ThoughtDef def, Pawn pawn, ref Hediff __result)
        {
            if (def == VPE_DefOf.EnvironmentDark && __result == null && DarkvisionPawns.Contains(pawn))
                __result = pawn.health.hediffSet.hediffs.OfType<Hediff_Darkvision>().FirstOrDefault();
        }

        [HarmonyPatch(typeof(StatPart_Glow), "ActiveFor")]
        [HarmonyPostfix]
        public static void NoDarkPenalty(Thing t, ref bool __result)
        {
            if (__result && t is Pawn p && DarkvisionPawns.Contains(p)) __result = false;
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            DarkvisionPawns.Add(this.pawn);
            foreach (BodyPartRecord part in this.pawn.RaceProps.body.AllParts.Where(part => part.def == BodyPartDefOf.Eye))
                this.pawn.health.AddHediff(VPE_DefOf.VPE_Darkvision_Display, part);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            DarkvisionPawns.Remove(this.pawn);
            Hediff hediff;
            while ((hediff = this.pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_Darkvision_Display)) != null) this.pawn.health.RemoveHediff(hediff);
        }
    }
}