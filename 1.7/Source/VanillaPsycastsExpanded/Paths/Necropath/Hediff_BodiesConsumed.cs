using Verse;

namespace VanillaPsycastsExpanded
{
    public class Hediff_BodiesConsumed : HediffWithComps
    {
        public override string Label => base.Label + ": " + consumedBodies;
        public int consumedBodies;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref consumedBodies, "consumedBodies");
        }
    }
}