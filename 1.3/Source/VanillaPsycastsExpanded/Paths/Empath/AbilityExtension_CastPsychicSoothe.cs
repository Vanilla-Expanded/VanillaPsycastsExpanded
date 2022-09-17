namespace VanillaPsycastsExpanded
{
using Mono.Unix.Native;
    using RimWorld;
    using RimWorld.Planet;
    using System.Collections.Generic;
    using Verse;
    using Verse.Sound;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;
	public class AbilityExtension_CastPsychicSoothe : AbilityExtension_AbilityMod
	{
		public Gender gender;
        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            var pawnsToApply = new List<GlobalTargetInfo>();
            foreach (Pawn pawn in ability.pawn.MapHeld.mapPawns.AllPawnsSpawned)
            {
                if (!pawn.Dead && pawn.gender == gender && pawn.needs != null && pawn.needs.mood != null)
                {
                    ability.ApplyHediff(pawn);
                }
            }
        }
	}
}