using RimWorld;
using RimWorld.Planet;
using VanillaPsycastsExpanded.Skipmaster;
using Verse;

namespace VanillaPsycastsExpanded.Nightstalker;

public class Ability_TeleportDark : Ability_Teleport
{
    public override FleckDef[] EffectSet =>
        new[]
        {
            VPE_DefOf.VPE_PsycastSkipFlashEntry_DarkBlue,
            FleckDefOf.PsycastSkipInnerExit,
            FleckDefOf.PsycastSkipOuterRingExit
        };

    public override bool CanHitTarget(LocalTargetInfo target) =>
        pawn.Map.glowGrid.GroundGlowAt(target.Cell) <= 0.29 && !target.Cell.Fogged(pawn.Map) && target.Cell.Walkable(pawn.Map);

    public override void ModifyTargets(ref GlobalTargetInfo[] targets)
    {
        base.ModifyTargets(ref targets);
        targets = new[] { pawn, targets[0] };
    }
}
