using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded.Chronopath;

public class AbilityExtension_Age : AbilityExtension_AbilityMod
{
    public float? casterYears = null;
    public float? targetYears = null;

    public override void Cast(GlobalTargetInfo[] targets, Ability ability)
    {
        base.Cast(targets, ability);
        if (casterYears.HasValue) Age(ability.pawn, casterYears.Value);
        if (!targetYears.HasValue) return;
        foreach (var target in targets)
            if (target.Thing is Pawn pawn)
                Age(pawn, targetYears.Value);
    }

    public override bool CanApplyOn(LocalTargetInfo target, Ability ability, bool throwMessages = false)
    {
        if (!base.CanApplyOn(target, ability, throwMessages)) return false;
        if (!targetYears.HasValue) return true;
        if (target.Thing is not Pawn pawn) return false;
        if (!pawn.RaceProps.IsFlesh) return false;
        if (!pawn.RaceProps.Humanlike) return false;
        return true;
    }

    public static void Age(Pawn pawn, float years)
    {
        pawn.ageTracker.AgeBiologicalTicks += Mathf.FloorToInt(years * GenDate.TicksPerYear);

        if (years < 0)
        {
            var giverSets = pawn.def.race.hediffGiverSets;
            if (giverSets == null) return;
            var lifeFraction = pawn.ageTracker.AgeBiologicalYears / pawn.def.race.lifeExpectancy;
            foreach (var giverSet in giverSets)
            foreach (var giver in giverSet.hediffGivers)
                if (giver is HediffGiver_Birthday giverBirthday)
                    if (giverBirthday.ageFractionChanceCurve.Evaluate(lifeFraction) <= 0f)
                    {
                        Hediff hediff;
                        while ((hediff = pawn.health.hediffSet.GetFirstHediffOfDef(giverBirthday.hediff)) != null) pawn.health.RemoveHediff(hediff);
                    }
        }

        if (pawn.ageTracker.AgeBiologicalYears > pawn.def.race.lifeExpectancy * 1.1f &&
            (pawn.genes == null || pawn.genes.HediffGiversCanGive(VPE_DefOf.HeartAttack)))
        {
            var part = pawn.RaceProps.body.AllParts.FirstOrDefault(p => p.def == BodyPartDefOf.Heart);
            var hediff = HediffMaker.MakeHediff(VPE_DefOf.HeartAttack, pawn, part);
            hediff.Severity = 1.1f;
            pawn.health.AddHediff(hediff, part);
        }
    }
}