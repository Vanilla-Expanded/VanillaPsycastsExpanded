using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using VEF.Abilities;
using Ability = VEF.Abilities.Ability;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_Flee : AbilityExtension_AbilityMod
{
    public bool onlyHostile = true;

    public override void Cast(GlobalTargetInfo[] targets, Ability ability)
    {
        base.Cast(targets, ability);
        foreach (var target in targets)
        {
            var pawn = target.Thing as Pawn;
            if (!onlyHostile || !pawn.HostileTo(ability.pawn)) return;
            pawn.GetLord()?.RemovePawn(pawn);
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, ability.def.label, true, false, false, null, true, false, true);
        }
    }
}
