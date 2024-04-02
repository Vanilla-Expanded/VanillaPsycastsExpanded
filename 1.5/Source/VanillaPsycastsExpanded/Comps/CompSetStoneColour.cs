namespace VanillaPsycastsExpanded
{
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    public class CompSetStoneColour : ThingComp
    {
        private ThingDef                      rockDef;
        public  CompProperties_SetStoneColour Props => (CompProperties_SetStoneColour) this.props;
        public Color color;

        public ThingDef KilledLeave => this.rockDef;

        public void SetStoneColour(ThingDef thingDef)
        {
            this.rockDef = thingDef;
            color = this.rockDef.graphic.data.color;
            Pawn pawn = this.parent as Pawn;
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref this.rockDef, nameof(this.rockDef));
            Scribe_Values.Look(ref color, nameof(this.color));
        }
    }
}