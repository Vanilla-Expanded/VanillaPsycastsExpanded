namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using Verse.AI;
    public class Ability_ConsumeBodies : Ability_TargetCorpse
    {
        public override void WarmupToil(Toil toil)
        {
            base.WarmupToil(toil);
            toil.AddPreInitAction(delegate
            {
                foreach (var target in Comp.currentlyCastingTargets)
                {
                    if (target.HasThing && target.Thing.TryGetComp<CompRottable>() != null)
                    {
                        this.AddEffecterToMaintain(VPE_DefOf.VPE_Liferot.Spawn(target.Thing.Position, this.pawn.Map), target.Thing, toil.defaultDuration);
                    }
                }
            });
            toil.AddPreTickAction(delegate
            {
                foreach (var target in Comp.currentlyCastingTargets)
                {
                    if (target.HasThing && target.Thing.TryGetComp<CompRottable>() != null && target.Thing.IsHashIntervalTick(60))
                    {
                        FilthMaker.TryMakeFilth(target.Thing.Position, target.Thing.Map, ThingDefOf.Filth_CorpseBile, 1);
                        target.Thing.TryGetComp<CompRottable>().RotProgress += 60000;
                    }
                }
            });
        }
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            if (!pawn.health.hediffSet.HasHediff(VPE_DefOf.VPE_BodiesConsumed))
            {
                pawn.health.AddHediff(VPE_DefOf.VPE_BodiesConsumed);
            }

            var consumerHediff = this.pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_BodiesConsumed) as Hediff_BodiesConsumed;
            foreach (var target in targets)
            {
                MoteBetween mote = (MoteBetween)ThingMaker.MakeThing(VPE_DefOf.VPE_SoulOrbTransfer);
                mote.Attach(target.Thing, this.pawn);
                mote.exactPosition = target.Thing.DrawPos;
                GenSpawn.Spawn(mote, target.Thing.Position, this.pawn.Map);
                consumerHediff.consumedBodies++;
                target.Thing.Destroy();
            }
        }
    }
}