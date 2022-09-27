namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Ability = VFECore.Abilities.Ability;

    public class MoteCastBubble : MoteBubble
    {
        private float durationSecs;

        protected override bool  EndOfLife => this.AgeSecs >= this.durationSecs;
        public override    float Alpha     => 1f;

        public void Setup(Pawn pawn, Ability ability)
        {
            this.SetupMoteBubble(ability.def.icon, null);
            this.Attach(pawn);
            this.durationSecs = Mathf.Max(3f, ability.GetCastTimeForPawn().TicksToSeconds());
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.durationSecs, nameof(this.durationSecs));
        }
    }
}