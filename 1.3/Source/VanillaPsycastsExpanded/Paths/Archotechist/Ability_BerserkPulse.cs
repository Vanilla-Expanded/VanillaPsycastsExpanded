namespace VanillaPsycastsExpanded
{
    using RimWorld.Planet;
    using Verse;
    using VFECore.Abilities;

    public class Ability_BerserkPulse : Ability
    {
        public IntVec3 targetCell;

        public override void ModifyTargets(ref GlobalTargetInfo[] targets)
        {
            this.targetCell = targets[0].Cell;
            base.ModifyTargets(ref targets);
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            MakeStaticFleck(this.targetCell, this.pawn.Map, VPE_DefOf.PsycastAreaEffect, this.GetRadiusForPawn(), 0);
        }
    }
}