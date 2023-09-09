using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded.Skipmaster;

public class Ability_Waterskip : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        var map = targets[0].Map;
        foreach (var c in AffectedCells(targets[0].Cell, map))
        {
            var thingList = c.GetThingList(map);
            for (var i = thingList.Count - 1; i >= 0; i--)
                switch (thingList[i])
                {
                    case Filth:
                    case Fire:
                        thingList[i].Destroy();
                        break;
                    case ThingWithComps twc when twc.TryGetComp<CompPower>() != null:
                    {
                        if (twc.TryGetComp<CompBreakdownable>() is { } comp1) comp1.DoBreakdown();
                        if (twc.TryGetComp<CompFlickable>() is { } comp2) comp2.SwitchIsOn = false;
                        if (twc.TryGetComp<CompProjectileInterceptor>() is not null || twc is Building_Turret)
                            twc.TakeDamage(new(DamageDefOf.EMP, 10, 10, -1, pawn));
                        break;
                    }
                }

            if (!c.Filled(map)) FilthMaker.TryMakeFilth(c, map, ThingDefOf.Filth_Water);

            var dataStatic = FleckMaker.GetDataStatic(c.ToVector3Shifted(), map, FleckDefOf.WaterskipSplashParticles);
            dataStatic.rotationRate = Rand.Range(-30, 30);
            dataStatic.rotation = 90 * Rand.RangeInclusive(0, 3);
            map.flecks.CreateFleck(dataStatic);
        }
    }

    private IEnumerable<IntVec3> AffectedCells(IntVec3 cell, Map map)
    {
        if (cell.Filled(pawn.Map)) yield break;

        foreach (var intVec in GenRadial.RadialCellsAround(cell, GetRadiusForPawn(), true))
            if (intVec.InBounds(map) && GenSight.LineOfSightToEdges(cell, intVec, map, true))
                yield return intVec;
    }

    public override void DrawHighlight(LocalTargetInfo target)
    {
        var range = GetRangeForPawn();
        if (GenRadial.MaxRadialPatternRadius > range && range >= 1)
            GenDraw.DrawRadiusRing(pawn.Position, range, Color.cyan);
        if (target.IsValid)
        {
            GenDraw.DrawTargetHighlight(target);
            GenDraw.DrawFieldEdges(AffectedCells(target.Cell, pawn.Map).ToList(), ValidateTarget(target, false) ? Color.white : Color.red);
        }
    }

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (target.Cell.Filled(pawn.Map))
        {
            if (showMessages)
                Messages.Message("AbilityOccupiedCells".Translate(def.LabelCap), target.ToTargetInfo(pawn.Map), MessageTypeDefOf.RejectInput,
                    false);

            return false;
        }

        return base.ValidateTarget(target, showMessages);
    }
}
