using Verse;

namespace VanillaPsycastsExpanded.Nightstalker;

public class HediffComp_DissapearsInLight : HediffComp
{
    public override bool CompShouldRemove => (Pawn.MapHeld?.glowGrid?.GroundGlowAt(Pawn.PositionHeld) ?? 0f) >= 0.21f;
}

public class HediffComp_DissapearsOnAttack : HediffComp
{
    public override bool CompShouldRemove =>
        Pawn?.stances?.curStance is Stance_Warmup { ticksLeft: <= 1, verb: { verbProps: { violent: true } } }
         or Stance_Cooldown { verb: { verbProps: { violent: true } } };
}
