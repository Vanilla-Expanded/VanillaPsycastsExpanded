namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using Verse;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_StealVitality : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            this.ApplyHediff(this.pawn, VPE_DefOf.VPE_GainedVitality, null, this.GetDurationForPawn(), 0);
        }
    }
}