namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;
	public class AbilityExtension_AbilityOffsetPrisonerResistance : AbilityExtension_AbilityMod
	{
		public float offset;
        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
			foreach (var target in targets)
            {
				Pawn pawn = target.Thing as Pawn;
				if (pawn != null)
				{
					var toOffset = offset * pawn.GetStatValue(StatDefOf.PsychicSensitivity);
					pawn.guest.resistance = Mathf.Max(pawn.guest.resistance + toOffset, 0f);
				}
			}
        }

        public override bool CanApplyOn(LocalTargetInfo target, Ability ability, bool throwMessages = false)
        {
			Pawn pawn = target.Pawn;
			if (pawn != null)
			{
				if (!pawn.IsPrisonerOfColony)
				{
					return false;
				}
				if (pawn != null && pawn.guest.resistance < float.Epsilon)
				{
					return false;
				}
				if (pawn.Downed)
				{
					return false;
				}
				return Valid( new[] { target.ToGlobalTargetInfo(target.Thing.Map) }, ability);
			}
			return false;
        }

        public override bool Valid(GlobalTargetInfo[] targets, Ability ability, bool throwMessages = false)
        {
			foreach (var target in targets)
            {
				Pawn pawn = target.Thing as Pawn;
				if (pawn != null && !AbilityUtility.ValidateHasResistance(pawn, throwMessages))
				{
					return false;
				}
			}
            return base.Valid(targets, ability, throwMessages);
        }
	}
}