namespace VanillaPsycastsExpanded.Wildspeaker
{
    using Verse;
    using Verse.Sound;

    public class CompProperties_PlaySound : CompProperties
    {
        public SoundDef sustainer;
        public SoundDef endSound;

        public CompProperties_PlaySound() => this.compClass = typeof(Comp_PlaySound);
    }

    public class Comp_PlaySound : ThingComp
    {
        private Sustainer                sustainer;
        private IntVec3                  cell;
        public  CompProperties_PlaySound Props => (CompProperties_PlaySound) this.props;

        public override void CompTick()
        {
            base.CompTick();
            if (!this.parent.Spawned) return;
            if (this.sustainer == null || this.sustainer.Ended)
                this.sustainer = this.Props.sustainer.TrySpawnSustainer(SoundInfo.InMap(this.parent, MaintenanceType.PerTick));
            if (this.Props.sustainer != null) this.sustainer.Maintain();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.cell = this.parent.Position;
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (this.Props.sustainer != null)
                if (!this.sustainer.Ended)
                    this.sustainer?.End();

            this.Props.endSound?.PlayOneShot(new TargetInfo(this.cell, map));
        }
    }
}