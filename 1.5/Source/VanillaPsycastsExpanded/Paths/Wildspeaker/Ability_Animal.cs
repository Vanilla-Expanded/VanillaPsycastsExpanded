using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded.Wildspeaker;

public class Ability_Animal : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (var target in targets)
            if (target.Thing is Pawn p)
                if (p.AnimalOrWildMan())
                {
                    var isManhunter = p.MentalStateDef == MentalStateDefOf.Manhunter || p.MentalStateDef == MentalStateDefOf.ManhunterPermanent;
                    if (Rand.Chance(GetSuccessChanceOn(p)))
                    {
                        if (isManhunter)
                            p.MentalState.RecoverFromState();
                        else
                            InteractionWorker_RecruitAttempt.DoRecruit(pawn, p);
                    }
                    else if (!isManhunter)
                        p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, "AnimalManhunterFromTaming".Translate(), true,
                            causedByPsycast: true);
                }
    }

    private float GetSuccessChanceOn(Pawn target) => pawn.GetStatValue(StatDefOf.PsychicSensitivity) - target.RaceProps.wildness;

    public override void OnGUI(LocalTargetInfo target)
    {
        base.OnGUI(target);
        var validTargets = currentTargets.Where(t => t is { IsValid: true, Map: not null }).ToList();
        var targets = new GlobalTargetInfo[validTargets.Count + 1];
        validTargets.CopyTo(targets, 0);
        targets[targets.Length - 1] = target.ToGlobalTargetInfo(validTargets?.LastOrDefault().Map ?? pawn.Map);
        ModifyTargets(ref targets);
        foreach (var targetInfo in targets)
            if (targetInfo.Thing is Pawn p)
            {
                if (p.AnimalOrWildMan() && GetSuccessChanceOn(p) > 1.401298E-45f)
                {
                    var chance = GetSuccessChanceOn(p);
                    var drawPos = p.DrawPos;
                    drawPos.z += 1f;
                    var color = chance switch
                    {
                        < 0.33f => Color.yellow,
                        < 0.66f => Color.white,
                        _ => Color.green
                    };
                    GenMapUI.DrawText(new(drawPos.x, drawPos.z), "VPE.SuccessChance".Translate() + ": " + chance.ToStringPercent(), color);
                }
                else
                {
                    var drawPos = p.DrawPos;
                    drawPos.z += 1f;
                    GenMapUI.DrawText(new(drawPos.x, drawPos.z), "Ineffective".Translate(), Color.red);
                }
            }
    }
}
