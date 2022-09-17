namespace VanillaPsycastsExpanded
{
using Mono.Unix.Native;
    using RimWorld;
    using RimWorld.Planet;
    using Verse;
    using Verse.Sound;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;
	public class AbilityExtension_TransferEntropy : AbilityExtension_AbilityMod
	{
		public bool targetReceivesEntropy = true;

        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
			foreach (var target in targets)
            {
				Pawn pawn = target.Thing as Pawn;
				if (pawn != null)
				{
					if (targetReceivesEntropy)
					{
						pawn.psychicEntropy.TryAddEntropy(ability.pawn.psychicEntropy.EntropyValue, ability.pawn, scale: false, overLimit: true);
					}
					if (!pawn.HasPsylink)
                    {
						var coma = HediffMaker.MakeHediff(VPE_DefOf.PsychicComa, pawn);
						pawn.health.AddHediff(coma);
					}
					ability.pawn.psychicEntropy.RemoveAllEntropy();
					MoteMaker.MakeInteractionOverlay(ThingDefOf.Mote_PsychicLinkPulse, ability.pawn, pawn);
				}
			}

		}

        public override bool IsEnabledForPawn(Ability ability, out string reason)
		{
			if (ability.pawn.psychicEntropy.EntropyValue <= 0f)
			{
				reason = "AbilityNoEntropyToDump".Translate();
				return false;
			}
			return base.IsEnabledForPawn(ability, out reason);

        }

        public override bool Valid(GlobalTargetInfo[] targets, Ability ability, bool throwMessages = false)
        {
			foreach (var target in targets)
            {
				Pawn pawn = target.Thing as Pawn;
				if (pawn != null && !AbilityUtility.ValidateNoMentalState(pawn, throwMessages))
				{
					return false;
				}
			}
			return base.Valid(targets, ability, throwMessages);
        }
	}
}