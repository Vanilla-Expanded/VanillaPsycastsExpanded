using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_CastPsychicSoothe : AbilityExtension_AbilityMod
{
    public Gender gender;

    public override void Cast(GlobalTargetInfo[] targets, Ability ability)
    {
        base.Cast(targets, ability);
        var pawnsToApply = new List<GlobalTargetInfo>();
        foreach (var pawn in ability.pawn.MapHeld.mapPawns.AllPawnsSpawned)
            if (!pawn.Dead && pawn.gender == gender && pawn.needs != null && pawn.needs.mood != null)
                ability.ApplyHediff(pawn);
    }
}