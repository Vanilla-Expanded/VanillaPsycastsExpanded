using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_PsycastWordOfSerenity : AbilityExtension_Psycast
{
    public List<MentalStateDef> exceptions;

    public float psyfocusCostForExtreme = -1f;

    public float psyfocusCostForMajor = -1f;
    public float psyfocusCostForMinor = -1f;

    public override void Cast(GlobalTargetInfo[] targets, Ability ability)
    {
        base.Cast(targets, ability);
        foreach (var target in targets)
        {
            var psycastHediff =
                (Hediff_PsycastAbilities)ability.pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_PsycastAbilityImplant);
            psycastHediff.UseAbility(PsyfocusCostForTarget(target), GetEntropyUsedByPawn(ability.pawn));
        }
    }

    public float PsyfocusCostForTarget(GlobalTargetInfo target)
    {
        return TargetMentalBreakIntensity(target) switch
        {
            MentalBreakIntensity.Minor => psyfocusCostForMinor,
            MentalBreakIntensity.Major => psyfocusCostForMajor,
            MentalBreakIntensity.Extreme => psyfocusCostForExtreme,
            _ => 0f
        };
    }

    public MentalBreakIntensity TargetMentalBreakIntensity(GlobalTargetInfo target)
    {
        var mentalStateDef = (target.Thing as Pawn)?.MentalStateDef;
        if (mentalStateDef != null)
        {
            var allDefsListForReading = DefDatabase<MentalBreakDef>.AllDefsListForReading;
            for (var i = 0; i < allDefsListForReading.Count; i++)
                if (allDefsListForReading[i].mentalState == mentalStateDef)
                    return allDefsListForReading[i].intensity;
        }

        return MentalBreakIntensity.Minor;
    }

    public override bool Valid(GlobalTargetInfo[] targets, Ability ability, bool throwMessages = false)
    {
        foreach (var target in targets)
        {
            var pawn = target.Thing as Pawn;
            if (pawn != null)
            {
                if (!AbilityUtility.ValidateHasMentalState(pawn, throwMessages, null)) return false;
                if (exceptions.Contains(pawn.MentalStateDef))
                {
                    if (throwMessages)
                        Messages.Message("AbilityDoesntWorkOnMentalState".Translate(ability.def.label, pawn.MentalStateDef.label), pawn,
                            MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                var num = PsyfocusCostForTarget(target);
                if (num > ability.pawn.psychicEntropy.CurrentPsyfocus + 0.0005f)
                {
                    var pawn2 = ability.pawn;
                    if (throwMessages)
                    {
                        var taggedString = ("MentalBreakIntensity" + TargetMentalBreakIntensity(target)).Translate();
                        Messages.Message(
                            "CommandPsycastNotEnoughPsyfocusForMentalBreak".Translate(num.ToStringPercent(), taggedString,
                                pawn2.psychicEntropy.CurrentPsyfocus.ToStringPercent("0.#"), ability.def.label.Named("PSYCASTNAME"), pawn2.Named("CASTERNAME")),
                            pawn, MessageTypeDefOf.RejectInput, false);
                    }

                    return false;
                }
            }
        }

        return base.Valid(targets, ability, throwMessages);
    }
}