using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded.Nightstalker;

public class Hediff_ShadowFocus : HediffWithComps
{
    public override HediffStage CurStage =>
        new()
        {
            statOffsets = new()
            {
                new()
                {
                    stat = StatDefOf.PsychicSensitivity,
                    value = 1f - pawn.MapHeld.glowGrid.GroundGlowAt(pawn.PositionHeld)
                }
            }
        };
}
