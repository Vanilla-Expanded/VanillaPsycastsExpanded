namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using Verse;
    public class StatPart_PsycastBodiesConsumed : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null)
                {
                    var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_BodiesConsumed) as Hediff_BodiesConsumed;
                    if (hediff != null && hediff.consumedBodies > 0)
                    {
                        val += hediff.consumedBodies;
                    }
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null)
                {
                    var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_BodiesConsumed) as Hediff_BodiesConsumed;
                    if (hediff != null && hediff.consumedBodies > 0)
                    {
                        return "VPE.StatsReport_BodiesConsumed".Translate(hediff.consumedBodies);
                    }
                }
            }
            return null;
        }
    }
}