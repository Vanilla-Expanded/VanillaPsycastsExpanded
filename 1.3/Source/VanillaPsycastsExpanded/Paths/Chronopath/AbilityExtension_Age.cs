namespace VanillaPsycastsExpanded.Chronopath;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

public class AbilityExtension_Age : AbilityExtension_AbilityMod
{
    public float? targetYears = null;
    public float? casterYears = null;

    public override void Cast(GlobalTargetInfo[] targets, Ability ability)
    {
        base.Cast(targets, ability);
        if (this.casterYears.HasValue) Age(ability.pawn, this.casterYears.Value);
        if (!this.targetYears.HasValue) return;
        foreach (GlobalTargetInfo target in targets)
            if (target.Thing is Pawn pawn)
                Age(pawn, this.targetYears.Value);
    }

    public override bool CanApplyOn(LocalTargetInfo target, Ability ability, bool throwMessages = false)
    {
        if (!base.CanApplyOn(target, ability, throwMessages)) return false;
        if (!this.targetYears.HasValue) return true;
        if (target.Thing is not Pawn pawn) return false;
        if (!pawn.RaceProps.IsFlesh) return false;
        if (!pawn.RaceProps.Humanlike) return false;
        return true;
    }

    public static void Age(Pawn pawn, float years)
    {
        if (years < 0 && pawn.ageTracker.AgeBiologicalYears <= 16) return;
        pawn.ageTracker.AgeTickMothballed((int)(years * GenDate.TicksPerYear));
        if (years < 0)
        {
            List<HediffGiverSetDef> giverSets = pawn.def.race.hediffGiverSets;
            if (giverSets == null) return;
            float lifeFraction = pawn.ageTracker.AgeBiologicalYears / pawn.def.race.lifeExpectancy;
            foreach (HediffGiverSetDef giverSet in giverSets)
            foreach (HediffGiver giver in giverSet.hediffGivers)
                if (giver is HediffGiver_Birthday giverBirthday)
                    if (giverBirthday.ageFractionChanceCurve.Evaluate(lifeFraction) <= 0f)
                    {
                        Hediff hediff;
                        while ((hediff = pawn.health.hediffSet.GetFirstHediffOfDef(giverBirthday.hediff)) != null) pawn.health.RemoveHediff(hediff);
                    }
        }

        if (pawn.ageTracker.AgeBiologicalYears < 16)
            pawn.ageTracker.AgeTickMothballed(GenDate.TicksPerYear * 16 - (int)pawn.ageTracker.AgeBiologicalTicks);

        if (pawn.ageTracker.AgeBiologicalYears > pawn.def.race.lifeExpectancy * 1.1f)
        {
            BodyPartRecord part   = pawn.RaceProps.body.AllParts.FirstOrDefault(p => p.def == BodyPartDefOf.Heart);
            Hediff         hediff = HediffMaker.MakeHediff(VPE_DefOf.HeartAttack, pawn, part);
            hediff.Severity = 1.1f;
            pawn.health.AddHediff(hediff, part);
        }
    }
}