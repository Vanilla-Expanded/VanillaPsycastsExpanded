using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded.Technomancer;

public class Ability_IncreaseQuality : Ability
{
    private QualityCategory MaxQuality => (QualityCategory)(int)GetPowerForPawn();

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (var target in targets)
        {
            var comp = target.Thing.GetInnerIfMinified().TryGetComp<CompQuality>();
            if (comp == null || comp.Quality >= MaxQuality) return;
            comp.SetQuality(comp.Quality + 1, ArtGenerationContext.Colony);
            for (var i = 0; i < 16; i++) FleckMaker.ThrowMicroSparks(target.Thing.TrueCenter(), pawn.Map);
        }
    }

    public override float GetPowerForPawn() =>
        pawn.GetStatValue(StatDefOf.PsychicSensitivity) switch
        {
            <= 1.2f => (int)QualityCategory.Good,
            <= 2.5f => (int)QualityCategory.Excellent,
            > 2.5f => (int)QualityCategory.Masterwork,
            _ => (int)QualityCategory.Normal
        };

    public override string GetPowerForPawnDescription() => "VPE.MaxQuality".Translate(MaxQuality.GetLabel()).Colorize(Color.cyan);

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (!base.ValidateTarget(target, showMessages)) return false;

        CompQuality comp;
        if ((comp = target.Thing.GetInnerIfMinified().TryGetComp<CompQuality>()) == null)
        {
            if (showMessages) Messages.Message("VPE.MustHaveQuality".Translate(), MessageTypeDefOf.RejectInput, false);
            return false;
        }

        if (comp.Quality >= MaxQuality)
        {
            if (showMessages) Messages.Message("VPE.QualityTooHigh".Translate(MaxQuality.GetLabel()), MessageTypeDefOf.RejectInput, false);
            return false;
        }

        return true;
    }
}
