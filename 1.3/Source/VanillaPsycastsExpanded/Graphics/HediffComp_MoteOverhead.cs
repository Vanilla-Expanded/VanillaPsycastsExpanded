namespace VanillaPsycastsExpanded.Graphics
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class HediffComp_MoteOverHead : HediffComp
    {
        private Mote mote;

        public HediffCompProperties_Mote Props => this.props as HediffCompProperties_Mote;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            Vector3 pos = this.Pawn.DrawPos + new Vector3(0f, 0f, 0.5f);
            if (this.mote == null || this.mote.Destroyed)
            {
                this.mote                         = MoteMaker.MakeStaticMote(pos, this.Pawn.Map, this.Props.mote);
                this.mote.Graphic.MatSingle.color = this.Props.color;
            }
            else
            {
                this.mote.exactPosition = pos;
                this.mote.Maintain();
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref this.mote, nameof(this.mote));
        }
    }

    public class HediffCompProperties_Mote : HediffCompProperties
    {
        public ThingDef mote;
        public Color    color;
    }
}