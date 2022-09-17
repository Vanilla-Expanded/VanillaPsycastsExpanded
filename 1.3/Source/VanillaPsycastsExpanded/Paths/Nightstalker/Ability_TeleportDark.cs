namespace VanillaPsycastsExpanded.Nightstalker
{
    using RimWorld;
    using RimWorld.Planet;
    using Skipmaster;
    using Verse;

    public class Ability_TeleportDark : Ability_Teleport
    {
        public override FleckDef[] EffectSet => new[]
        {
            VPE_DefOf.VPE_PsycastSkipFlashEntry_DarkBlue,
            FleckDefOf.PsycastSkipInnerExit,
            FleckDefOf.PsycastSkipOuterRingExit
        };

        public override bool CanHitTarget(LocalTargetInfo target) =>
            this.pawn.Map.glowGrid.GameGlowAt(target.Cell) <= 0.29 && !target.Cell.Fogged(this.pawn.Map) && target.Cell.Walkable(this.pawn.Map);

        public override void ModifyTargets(ref GlobalTargetInfo[] targets)
        {
            base.ModifyTargets(ref targets);
            targets = new[] {this.pawn, targets[0]};
        }
    }
}