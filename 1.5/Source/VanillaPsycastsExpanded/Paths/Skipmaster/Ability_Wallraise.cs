namespace VanillaPsycastsExpanded.Skipmaster;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

public class Ability_Wallraise : Ability
{
    public AbilityExtension_Wallraise Props => this.def.GetModExtension<AbilityExtension_Wallraise>();

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
        {
            Map             map         = target.Map;
            LocalTargetInfo localTarget = target.HasThing ? new LocalTargetInfo(target.Thing) : new LocalTargetInfo(target.Cell);
            List<Thing>     items       = new();
            items.AddRange(this.Props.AffectedCells(localTarget, map).SelectMany(c => from t in c.GetThingList(map)
                                                                                      where t.def.category == ThingCategory.Item
                                                                                      select t));
            foreach (Thing item in items) item.DeSpawn();

            foreach (IntVec3 loc in this.Props.AffectedCells(localTarget, map))
            {
                GenSpawn.Spawn(ThingDefOf.RaisedRocks, loc, map);
                FleckMaker.ThrowDustPuffThick(loc.ToVector3Shifted(), map, Rand.Range(1.5f, 3f), CompAbilityEffect_Wallraise.DustColor);
            }

            foreach (Thing item in items)
            {
                IntVec3 cell = IntVec3.Invalid;
                for (int i = 0; i < 9; i++)
                {
                    IntVec3 c = item.Position + GenRadial.RadialPattern[i];
                    if (c.InBounds(map) && c.Walkable(map) && map.thingGrid.ThingsListAtFast(c).Count <= 0)
                    {
                        cell = c;
                        break;
                    }
                }

                if (cell != IntVec3.Invalid)
                    GenSpawn.Spawn(item, cell, map);
                else
                    GenPlace.TryPlaceThing(item, item.Position, map, ThingPlaceMode.Near);
            }
        }
    }

    public override void DrawHighlight(LocalTargetInfo target)
    {
        base.DrawHighlight(target);
        GenDraw.DrawFieldEdges(this.Props.AffectedCells(target, this.pawn.Map).ToList(), this.ValidateTarget(target) ? Color.white : Color.red);
    }

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = false)
    {
        if (this.Props.AffectedCells(target, this.pawn.Map).Any(c => c.Filled(this.pawn.Map)))
        {
            if (showMessages)
                Messages.Message("AbilityOccupiedCells".Translate(this.def.LabelCap), target.ToTargetInfo(this.pawn.Map),
                                 MessageTypeDefOf.RejectInput, false);

            return false;
        }

        if (this.Props.AffectedCells(target, this.pawn.Map).Any(c => !c.Standable(this.pawn.Map)))
        {
            if (showMessages)
                Messages.Message("AbilityUnwalkable".Translate(this.def.LabelCap), target.ToTargetInfo(this.pawn.Map),
                                 MessageTypeDefOf.RejectInput, false);

            return false;
        }

        return true;
    }
}

public class AbilityExtension_Wallraise : AbilityExtension_AbilityMod
{
    public List<IntVec2> pattern;
    public float         screenShakeIntensity;

    internal IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map) =>
        this.pattern.Select(intVec => target.Cell + new IntVec3(intVec.x, 0, intVec.z)).Where(intVec2 => intVec2.InBounds(map));
}