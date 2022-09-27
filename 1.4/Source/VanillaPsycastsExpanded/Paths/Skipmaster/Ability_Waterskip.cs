namespace VanillaPsycastsExpanded.Skipmaster
{
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_Waterskip : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            Map map = targets[0].Map;
            foreach (IntVec3 c in this.AffectedCells(targets[0].Cell, map))
            {
                List<Thing> thingList = c.GetThingList(map);
                for (int i = thingList.Count - 1; i >= 0; i--)
                    switch (thingList[i])
                    {
                        case Fire:
                            thingList[i].Destroy();
                            break;
                        case ThingWithComps twc when twc.TryGetComp<CompPower>() != null:
                        {
                            if (twc.TryGetComp<CompBreakdownable>() is { } comp1) comp1.DoBreakdown();
                            if (twc.TryGetComp<CompFlickable>() is { } comp2) comp2.SwitchIsOn = false;
                            if (twc.TryGetComp<CompProjectileInterceptor>() is not null || twc is Building_Turret)
                                twc.TakeDamage(new DamageInfo(DamageDefOf.EMP, 10, 10, -1, this.pawn));
                            break;
                        }
                    }

                if (!c.Filled(map)) FilthMaker.TryMakeFilth(c, map, ThingDefOf.Filth_Water);

                FleckCreationData dataStatic = FleckMaker.GetDataStatic(c.ToVector3Shifted(), map, FleckDefOf.WaterskipSplashParticles);
                dataStatic.rotationRate = Rand.Range(-30, 30);
                dataStatic.rotation     = 90 * Rand.RangeInclusive(0, 3);
                map.flecks.CreateFleck(dataStatic);
            }
        }

        private IEnumerable<IntVec3> AffectedCells(IntVec3 cell, Map map)
        {
            if (cell.Filled(this.pawn.Map)) yield break;

            foreach (IntVec3 intVec in GenRadial.RadialCellsAround(cell, this.GetRadiusForPawn(), true))
                if (intVec.InBounds(map) && GenSight.LineOfSightToEdges(cell, intVec, map, true))
                    yield return intVec;
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            float range = this.GetRangeForPawn();
            if (GenRadial.MaxRadialPatternRadius > range && range >= 1)
                GenDraw.DrawRadiusRing(this.pawn.Position, range, Color.cyan);
            if (target.IsValid)
            {
                GenDraw.DrawTargetHighlight(target);
                GenDraw.DrawFieldEdges(this.AffectedCells(target.Cell, this.pawn.Map).ToList(), this.ValidateTarget(target, false) ? Color.white : Color.red);
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (target.Cell.Filled(this.pawn.Map))
            {
                if (showMessages)
                    Messages.Message("AbilityOccupiedCells".Translate(this.def.LabelCap), target.ToTargetInfo(this.pawn.Map), MessageTypeDefOf.RejectInput,
                                     false);

                return false;
            }

            return base.ValidateTarget(target, showMessages);
        }
    }
}