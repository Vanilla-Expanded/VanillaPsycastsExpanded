namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using System.Linq;
    using Verse;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;
    public class AbilityExtension_WordOfLove : AbilityExtension_AbilityMod
	{
        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
			Pawn target = targets[1].Thing as Pawn;
			Pawn pawn = targets[0].Thing as Pawn;
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicLove);
			if (firstHediffOfDef != null)
			{
				pawn.health.RemoveHediff(firstHediffOfDef);
			}
			Hediff_PsychicLove hediff_PsychicLove = (Hediff_PsychicLove)HediffMaker.MakeHediff(HediffDefOf.PsychicLove, pawn, pawn.health.hediffSet.GetBrain());
			hediff_PsychicLove.target = target;
			HediffComp_Disappears hediffComp_Disappears = hediff_PsychicLove.TryGetComp<HediffComp_Disappears>();
			if (hediffComp_Disappears != null)
			{
				hediffComp_Disappears.ticksToDisappear = (int)(ability.GetDurationForPawn() * pawn.GetStatValue(StatDefOf.PsychicSensitivity));
			}
			pawn.health.AddHediff(hediff_PsychicLove);
		}

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target, Ability ability)
		{
			var targets = ability.currentTargets.Where(x => x.Thing != null).ToList();
			if (targets.Any())
			{
				return "PsychicLoveFor".Translate();
			}
			return "PsychicLoveInduceIn".Translate();
		}
		public override bool HidePawnTooltips => true;
        public override bool ValidateTarget(LocalTargetInfo target, Ability ability, bool showMessages = true)
		{
			var targets = ability.currentTargets.Where(x => x.Thing != null).ToList();
			if (targets.Any())
            {
				Pawn pawn = targets[0].Thing as Pawn;
				Pawn pawn2 = target.Pawn;
				if (pawn == pawn2)
				{
					return false;
				}
				if (pawn != null && pawn2 != null && !pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
				{
					Gender gender = pawn.gender;
					Gender gender2 = (pawn.story.traits.HasTrait(TraitDefOf.Gay) ? gender : gender.Opposite());
					if (pawn2.gender != gender2)
					{
						if (showMessages)
						{
							Messages.Message("AbilityCantApplyWrongAttractionGender".Translate(pawn, pawn2), pawn, MessageTypeDefOf.RejectInput, historical: false);
						}
						return false;
					}
				}
				return true;
			}
			return base.ValidateTarget(target, ability, showMessages);
		}

        public override bool Valid(GlobalTargetInfo[] targets, Ability ability, bool throwMessages = false)
        {
			foreach (var target in targets)
            {
				Pawn pawn = target.Thing as Pawn;
				if (pawn != null)
				{
					if (pawn.story.traits.HasTrait(TraitDefOf.Asexual))
					{
						if (throwMessages)
						{
							Messages.Message("AbilityCantApplyOnAsexual".Translate(ability.def.label), pawn, MessageTypeDefOf.RejectInput, historical: false);
						}
						return false;
					}
					if (!AbilityUtility.ValidateNoMentalState(pawn, throwMessages))
					{
						return false;
					}
				}
			}
            return base.Valid(targets, ability, throwMessages);
        }
	}
}