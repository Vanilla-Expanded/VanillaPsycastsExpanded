namespace VanillaPsycastsExpanded
{
    using Verse;
    using Verse.Sound;
    public class HediffCompProperties_PlaySound : HediffCompProperties
    {
        public SoundDef sustainer;
        public SoundDef endSound;
        public HediffCompProperties_PlaySound()
        {
            compClass = typeof(HediffComp_PlaySound);
        }
    }
    public class HediffComp_PlaySound : HediffComp
    {
        private Sustainer sustainer;
        public HediffCompProperties_PlaySound Props => (HediffCompProperties_PlaySound)props;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Props.sustainer != null)
            {
                if (sustainer == null || sustainer.Ended)
                {
                    sustainer = Props.sustainer.TrySpawnSustainer(SoundInfo.InMap(Pawn, MaintenanceType.PerTick));
                }
                sustainer.Maintain();
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (Props.sustainer != null)
            {
                if (!sustainer.Ended)
                {
                    sustainer?.End();
                }
            }
            if (Props.endSound != null)
            {
                Props.endSound.PlayOneShot(base.Pawn);
            }
        }
    }
}