namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;
	public class AbilityExtension_JoinFaction : AbilityExtension_AbilityMod
	{
        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            foreach (var target in targets)
            {
                if (target.Thing.Faction != ability.pawn.Faction)
                {
                    target.Thing.SetFaction(ability.pawn.Faction);
                }
            }
        }
    }
}