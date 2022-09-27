namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using Verse;

    public class SoulFromSky : Skyfaller
    {
        public Corpse target;
        protected override void Impact()
        {
            var pawn = target.InnerPawn;
            var hediffs = pawn.health.hediffSet.hediffs;
            for (var i = hediffs.Count - 1; i >= 0; i--)
            {
                var hediff = hediffs[i];
                if (hediff is Hediff_MissingPart hediff_MissingPart)
                {
                    var part = hediff_MissingPart.Part;
                    pawn.health.RemoveHediff(hediff);
                    pawn.health.RestorePart(part);
                }
                else if (hediff.def != VPE_DefOf.TraumaSavant && (hediff.def.isBad || hediff is Hediff_Addiction) && hediff.def.everCurableByItem)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
            ResurrectionUtility.ResurrectWithSideEffects(pawn);
            if (!pawn.Spawned)
            {
                GenSpawn.Spawn(pawn, this.Position, this.MapHeld);
            }
            Destroy();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref target, "target");
        }
    }
}