namespace VanillaPsycastsExpanded;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

public class FocusStrengthOffset_NearbyOfTechlevel : FocusStrengthOffset
{
    public TechLevel techLevel;
    public float     radius = 10f;

    public override float GetOffset(Thing parent, Pawn user = null)
    {
        int   count;
        float x;
        if (parent.Map == null)
        {
            count = 1;
            x     = parent.MarketValue * parent.stackCount;
        }
        else
        {
            List<Thing> things = this.GetThings(parent.Position, parent.Map);
            count = Mathf.Clamp(things.Count,                                  1, 10);
            x     = Mathf.Clamp(things.Sum(t => t.MarketValue * t.stackCount), 1, 5000);
        }

        return count / 5.55f * x / 10000f;
    }

    public override string GetExplanation(Thing parent)
    {
        int num = parent.Map == null ? 1 : this.GetThings(parent.Position, parent.Map).Count;
        return "VPE.ThingsOfLevel".Translate(num, this.techLevel.ToString()) + ": " + this.GetOffset(parent).ToStringWithSign("0%");
    }

    public override void PostDrawExtraSelectionOverlays(Thing parent, Pawn user = null)
    {
        base.PostDrawExtraSelectionOverlays(parent, user);
        GenDraw.DrawRadiusRing(parent.Position, this.radius, PlaceWorker_MeditationOffsetBuildingsNear.RingColor);
        foreach (Thing thing in this.GetThings(parent.Position, parent.Map))
            GenDraw.DrawLineBetween(parent.TrueCenter(), thing.TrueCenter(), SimpleColor.Green);
    }

    protected virtual List<Thing> GetThings(IntVec3 cell, Map map)
    {
        return GenRadialCached.RadialDistinctThingsAround(cell, map, this.radius, true).Where(t => t.def.techLevel == this.techLevel).Take(10).ToList();
    }
}