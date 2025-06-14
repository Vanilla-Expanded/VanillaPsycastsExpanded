namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using Verse;

    public class CompMelter : ThingComp
    {
        public float damageBuffer;
        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.IsHashIntervalTick(60))
            {
                float ambientTemperature = parent.AmbientTemperature;
                if (ambientTemperature > 0)
                {
                    damageBuffer += ambientTemperature / 41.66f;
                    if (damageBuffer >= 1f)
                    {
                        this.parent.HitPoints -= (int)damageBuffer;
                        damageBuffer = 0f;
                    }
                    if (this.parent.HitPoints < 0)
                    {
                        FilthMaker.TryMakeFilth(this.parent.Position, this.parent.Map, ThingDefOf.Filth_Water);
                        this.parent.Destroy();
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref damageBuffer, "damageBuffer");
        }
    }
}