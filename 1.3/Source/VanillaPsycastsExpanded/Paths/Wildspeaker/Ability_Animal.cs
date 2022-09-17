namespace VanillaPsycastsExpanded.Wildspeaker;

using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

public class Ability_Animal : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
            if (target.Thing is Pawn pawn)
                if (pawn.AnimalOrWildMan())
                {
                    if (pawn.MentalStateDef == MentalStateDefOf.Manhunter || pawn.MentalStateDef == MentalStateDefOf.ManhunterPermanent)
                        pawn.MentalState.RecoverFromState();
                    else
                        InteractionWorker_RecruitAttempt.DoRecruit(this.pawn, pawn);
                }
    }
}