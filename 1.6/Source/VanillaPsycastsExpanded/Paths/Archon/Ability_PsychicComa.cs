namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using VEF.Abilities;
    using Ability = VEF.Abilities.Ability;
    public class Ability_PsychicComa : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            // If a psychic coma ability extension is present then let
            // it handle this instead. It has more customization to it.
            // This class is basically for abilities using it for compatibility reasons.
            if (!def.HasModExtension<AbilityExtension_PsychicComa>())
            {
                var coma = HediffMaker.MakeHediff(VPE_DefOf.PsychicComa, this.pawn);
                coma.TryGetComp<HediffComp_Disappears>().ticksToDisappear = (int)((GenDate.TicksPerDay * 5) / this.pawn.GetStatValue(StatDefOf.PsychicSensitivity));
                this.pawn.health.AddHediff(coma);
            }
        }
    }
}