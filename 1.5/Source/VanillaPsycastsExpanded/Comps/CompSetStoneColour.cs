using UnityEngine;
using Verse;

namespace VanillaPsycastsExpanded;

[StaticConstructorOnStartup]
public class CompSetStoneColour : ThingComp
{
    public Color color;
    private ThingDef rockDef;

    public ThingDef KilledLeave => rockDef;

    public void SetStoneColour(ThingDef thingDef)
    {
        rockDef = thingDef;
        color = rockDef.graphic.data.color;
        var pawn = parent as Pawn;
        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Defs.Look(ref rockDef, nameof(rockDef));
        Scribe_Values.Look(ref color, nameof(color));
    }
}
