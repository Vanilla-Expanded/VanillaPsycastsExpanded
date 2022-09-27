namespace VanillaPsycastsExpanded.Nightstalker
{
    using System.Collections.Generic;
    using RimWorld;
    using Verse;

    public class Hediff_ShadowFocus : HediffWithComps
    {
        public override HediffStage CurStage => new()
        {
            statOffsets = new List<StatModifier>
            {
                new()
                {
                    stat  = StatDefOf.PsychicSensitivity,
                    value = 1f - this.pawn.MapHeld.glowGrid.GameGlowAt(this.pawn.PositionHeld)
                }
            }
        };
    }
}