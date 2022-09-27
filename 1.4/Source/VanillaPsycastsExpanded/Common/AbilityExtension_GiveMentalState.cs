using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_GiveMentalState : AbilityExtension_AbilityMod
{
    public bool applyToSelf;
    public bool clearOthers;

    public StatDef durationMultiplier;
    public bool durationScalesWithCaster;
    public MentalStateDef stateDef;

    public MentalStateDef stateDefForMechs;

    public override void Cast(GlobalTargetInfo[] targets, Ability ability)
    {
        base.Cast(targets, ability);
        foreach (var target in targets)
        {
            var pawn = applyToSelf ? ability.pawn : target.Thing as Pawn;
            if (pawn is null) continue;
            if (pawn.InMentalState)
            {
                if (clearOthers) pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
                else continue;
            }

            TryGiveMentalStateWithDuration(pawn.RaceProps.IsMechanoid ? stateDefForMechs ?? stateDef : stateDef,
                pawn, ability, durationMultiplier, durationScalesWithCaster);
            RestUtility.WakeUp(pawn);
        }
    }

    public override bool Valid(GlobalTargetInfo[] targets, Ability ability, bool throwMessages = false)
    {
        var pawn = targets.Select(t => t.Thing).OfType<Pawn>().FirstOrDefault();
        if (pawn != null && !AbilityUtility.ValidateNoMentalState(pawn, throwMessages, null)) return false;
        return true;
    }

    public static void TryGiveMentalStateWithDuration(MentalStateDef def, Pawn p, Ability ability, StatDef multiplierStat, bool durationScalesWithCaster)
    {
        if (p.mindState.mentalStateHandler.TryStartMentalState(def, null, true, false, null, false,
                false, ability.def.GetModExtension<AbilityExtension_Psycast>() != null))
        {
            float num = ability.GetDurationForPawn();
            if (multiplierStat != null)
            {
                if (durationScalesWithCaster)
                    num *= p.GetStatValue(multiplierStat);
                else
                    num *= ability.pawn.GetStatValue(multiplierStat);
            }

            p.mindState.mentalStateHandler.CurState.forceRecoverAfterTicks = (int)num;
        }
    }
}