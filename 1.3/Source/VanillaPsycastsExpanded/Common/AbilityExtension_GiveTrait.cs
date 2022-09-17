namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;
	public class AbilityExtension_GiveTrait : AbilityExtension_AbilityMod
	{
        public TraitDef trait;
        public int degree;
        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            foreach (var target in targets)
            {
                var pawn = target.Thing as Pawn;
                if (!pawn.story?.traits?.HasTrait(trait, degree) ?? false)
                {
                    pawn.story.traits.GainTrait(new Trait(trait, degree));
                    pawn.needs.AddOrRemoveNeedsAsAppropriate(); // for thrall
                }
            }
        }
    }
}