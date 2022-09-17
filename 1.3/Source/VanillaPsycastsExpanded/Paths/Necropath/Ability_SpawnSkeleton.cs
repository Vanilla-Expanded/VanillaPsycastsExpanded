namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using Verse.AI;
    using VFECore.Abilities;
    public class Ability_SpawnSkeleton : Ability_TargetCorpse
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            foreach (var target in targets)
            {
                var corpse = target.Thing as Corpse;
                var pos = corpse.Position;
                corpse.Destroy();
                FilthMaker.TryMakeFilth(pos, this.pawn.Map, ThingDefOf.Filth_CorpseBile, 3);
                GenSpawn.Spawn(PawnGenerator.GeneratePawn(VPE_DefOf.VPE_SummonedSkeleton, this.pawn.Faction), pos, this.pawn.Map);
            }
        }
    }
}