namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using System.Linq;
    using Verse;

    public class Hediff_Liferot : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();
            if (this.pawn.IsHashIntervalTick(60) && this.pawn.health.hediffSet.GetNotMissingParts().Where(x => x.coverageAbs > 0)
                .TryRandomElement(out var part))
            {
                this.pawn.TakeDamage(new DamageInfo(VPE_DefOf.VPE_Rot, 1, hitPart: part));
            }
        }
    }
}