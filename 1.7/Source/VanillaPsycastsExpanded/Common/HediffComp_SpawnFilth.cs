namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using Verse;
    public class HediffCompProperties_SpawnFilth : HediffCompProperties
    {
        public ThingDef filthDef;
        public int intervalRate;
        public IntRange filthCount;
        public HediffCompProperties_SpawnFilth()
        {
            compClass = typeof(HediffComp_SpawnFilth);
        }
    }
    public class HediffComp_SpawnFilth : HediffComp
    {
        public HediffCompProperties_SpawnFilth Props => base.props as HediffCompProperties_SpawnFilth;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (this.Pawn.Spawned && this.Pawn.IsHashIntervalTick(Props.intervalRate))
            {
                FilthMaker.TryMakeFilth(this.Pawn.Position, this.Pawn.Map, Props.filthDef,
                    Props.filthCount.RandomInRange);
            }
        }
    }
}
