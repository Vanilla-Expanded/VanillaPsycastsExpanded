using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded.Technomancer;

public class Ability_AffectMechs : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (var target in targets)
        foreach (var thing in AllTargetsAt(target.Cell, target.Map).InRandomOrder().Take(3))
        {
            ApplyHediffs(new GlobalTargetInfo(thing));
            if (thing.TryGetComp<CompHaywire>() is { } comp) comp.GoHaywire(GetDurationForPawn());
        }
    }

    public override void DrawHighlight(LocalTargetInfo target)
    {
        base.DrawHighlight(target);
        foreach (var thing in AllTargetsAt(target.Cell)) GenDraw.DrawTargetHighlight(thing);
    }

    private IEnumerable<Thing> AllTargetsAt(IntVec3 cell, Map map = null)
    {
        foreach (var thing in GenRadial.RadialDistinctThingsAround(cell, map ?? pawn.Map, GetRadiusForPawn(), true))
        {
            if (thing is Building_Turret) yield return thing;
            if (thing is Pawn { RaceProps: { IsMechanoid: true } }) yield return thing;
        }
    }
}
