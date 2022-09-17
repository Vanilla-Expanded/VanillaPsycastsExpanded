namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;
    public class Ability_PsychicComa : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var coma = HediffMaker.MakeHediff(VPE_DefOf.PsychicComa, this.pawn);
            coma.TryGetComp<HediffComp_Disappears>().ticksToDisappear = (int)((GenDate.TicksPerDay * 5) / this.pawn.GetStatValue(StatDefOf.PsychicSensitivity));
            this.pawn.health.AddHediff(coma);
        }
    }
}