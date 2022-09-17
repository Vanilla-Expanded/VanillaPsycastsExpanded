namespace VanillaPsycastsExpanded.Technomancer;

using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

public class Ability_AffectMechs : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
        foreach (Thing thing in this.AllTargetsAt(target.Cell, target.Map))
        {
            this.ApplyHediffs(new GlobalTargetInfo(thing));
            if (thing.TryGetComp<CompHaywire>() is { } comp) comp.GoHaywire(this.GetDurationForPawn());
        }
    }

    public override void DrawHighlight(LocalTargetInfo target)
    {
        base.DrawHighlight(target);
        foreach (Thing thing in this.AllTargetsAt(target.Cell)) GenDraw.DrawTargetHighlight(thing);
    }

    private IEnumerable<Thing> AllTargetsAt(IntVec3 cell, Map map = null)
    {
        foreach (Thing thing in GenRadial.RadialDistinctThingsAround(cell, map ?? this.pawn.Map, this.GetRadiusForPawn(), true))
        {
            if (thing is Building_Turret) yield return thing;
            if (thing is Pawn { RaceProps: { IsMechanoid: true } }) yield return thing;
        }
    }
}