using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaPsycastsExpanded;

public class FocusStrengthOffset_NearbyOfTechlevel : FocusStrengthOffset
{
    public float radius = 10f;
    public TechLevel techLevel;

    public override float GetOffset(Thing parent, Pawn user = null)
    {
        int count;
        float x;
        if (parent.MapHeld is { } map)
        {
            var things = GetThings(parent.Position, map);
            count = Mathf.Clamp(things.Count, 1, 10);
            x = Mathf.Clamp(things.Sum(t => t.MarketValue * t.stackCount), 1, 5000);
        }
        else
        {
            count = 1;
            x = parent.MarketValue * parent.stackCount;
        }

        return count / 5.55f * x / 10000f;
    }

    public override string GetExplanation(Thing parent)
    {
        var num = parent.MapHeld is { } map ? GetThings(parent.Position, map).Count : 1;
        return "VPE.ThingsOfLevel".Translate(num, techLevel.ToString()) + ": " + GetOffset(parent).ToStringWithSign("0%");
    }

    public override void PostDrawExtraSelectionOverlays(Thing parent, Pawn user = null)
    {
        base.PostDrawExtraSelectionOverlays(parent, user);
        GenDraw.DrawRadiusRing(parent.Position, radius, PlaceWorker_MeditationOffsetBuildingsNear.RingColor);
        if (parent.MapHeld is { } map)
            foreach (var thing in GetThings(parent.Position, map))
                GenDraw.DrawLineBetween(parent.TrueCenter(), thing.TrueCenter(), SimpleColor.Green);
    }

    protected virtual List<Thing> GetThings(IntVec3 cell, Map map)
    {
        return GenRadialCached.RadialDistinctThingsAround(cell, map, radius, true).Where(t => t.def.techLevel == techLevel).Take(10).ToList();
    }
}
