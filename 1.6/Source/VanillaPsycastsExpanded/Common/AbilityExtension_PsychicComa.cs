namespace VanillaPsycastsExpanded;

using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VEF.Abilities;
using Ability = VEF.Abilities.Ability;

public class AbilityExtension_PsychicComa : AbilityExtension_AbilityMod
{
    public float     hours;
    public HediffDef coma;
    public StatDef   multiplier;
    public int       ticks;
    public bool      autoApply = true;

    public virtual int GetComaDuration(Ability ability)
    {
        float duration = this.hours * GenDate.TicksPerHour + this.ticks;
        float mult     = ability.pawn.GetStatValue(this.multiplier ?? StatDefOf.PsychicSensitivity);

        duration *= Mathf.Approximately(mult, 0f) ? 10f : 1 / mult;
        return Mathf.FloorToInt(duration);
    }

    public virtual void ApplyComa(Ability ability)
    {
        int duration = GetComaDuration(ability);

        if (duration > 0)
        {
            Hediff hediff = HediffMaker.MakeHediff(this.coma ?? VPE_DefOf.PsychicComa, ability.pawn);
            hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = duration;
            ability.pawn.health.AddHediff(hediff);
        }
    }

    public override void Cast(GlobalTargetInfo[] targets, Ability ability)
    {
        base.Cast(targets, ability);

        if (autoApply)
        {
            ApplyComa(ability);
        }
    }

    public override string GetDescription(Ability ability)
    {
        int duration = GetComaDuration(ability);

        if (duration > 0)
            return $"{"VPE.PsychicComaDuration".Translate()}: {duration.ToStringTicksToPeriod(false)}".Colorize(Color.red);

        return string.Empty;
    }
}