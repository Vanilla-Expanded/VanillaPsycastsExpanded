using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_WordOfLove : AbilityExtension_AbilityMod
{
    public override bool HidePawnTooltips => true;

    public override void Cast(GlobalTargetInfo[] targets, Ability ability)
    {
        base.Cast(targets, ability);
        var target = targets[1].Thing as Pawn;
        var pawn = targets[0].Thing as Pawn;
        var firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicLove);
        if (firstHediffOfDef != null) pawn.health.RemoveHediff(firstHediffOfDef);
        var hediff_PsychicLove = (Hediff_PsychicLove)HediffMaker.MakeHediff(HediffDefOf.PsychicLove, pawn, pawn.health.hediffSet.GetBrain());
        hediff_PsychicLove.target = target;
        var hediffComp_Disappears = hediff_PsychicLove.TryGetComp<HediffComp_Disappears>();
        if (hediffComp_Disappears != null)
            hediffComp_Disappears.ticksToDisappear = (int)(ability.GetDurationForPawn() * pawn.GetStatValue(StatDefOf.PsychicSensitivity));
        pawn.health.AddHediff(hediff_PsychicLove);
    }

    public override string ExtraLabelMouseAttachment(LocalTargetInfo target, Ability ability)
    {
        var targets = ability.currentTargets.Where(x => x.Thing != null).ToList();
        if (targets.Any()) return "PsychicLoveFor".Translate();
        return "PsychicLoveInduceIn".Translate();
    }

    public override bool ValidateTarget(LocalTargetInfo target, Ability ability, bool showMessages = true)
    {
        var targets = ability.currentTargets.Where(x => x.Thing != null).ToList();
        if (targets.Any())
        {
            var pawn = targets[0].Thing as Pawn;
            var pawn2 = target.Pawn;
            if (pawn == pawn2) return false;
            if (pawn != null && pawn2 != null && !pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
            {
                var gender = pawn.gender;
                var gender2 = pawn.story.traits.HasTrait(TraitDefOf.Gay) ? gender : gender.Opposite();
                if (pawn2.gender != gender2)
                {
                    if (showMessages)
                        Messages.Message("AbilityCantApplyWrongAttractionGender".Translate(pawn, pawn2), pawn, MessageTypeDefOf.RejectInput, false);
                    return false;
                }
            }

            return true;
        }

        return base.ValidateTarget(target, ability, showMessages);
    }

    public override bool Valid(GlobalTargetInfo[] targets, Ability ability, bool throwMessages = false)
    {
        foreach (var target in targets)
        {
            var pawn = target.Thing as Pawn;
            if (pawn != null)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Asexual))
                {
                    if (throwMessages) Messages.Message("AbilityCantApplyOnAsexual".Translate(ability.def.label), pawn, MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                if (!AbilityUtility.ValidateNoMentalState(pawn, throwMessages, null)) return false;
            }
        }

        return base.Valid(targets, ability, throwMessages);
    }
}