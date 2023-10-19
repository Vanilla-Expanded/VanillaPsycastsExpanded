using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;
using AbilityDef = VFECore.Abilities.AbilityDef;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_Psycast : AbilityExtension_AbilityMod
{
    public float entropyGain = 0f;
    public List<StatModifier> entropyGainStatFactors = new();
    public int level;
    public int order;
    public PsycasterPathDef path;
    public List<AbilityDef> prerequisites = new();
    public bool psychic;
    public float psyfocusCost = 0f;
    public bool showCastBubble = true;
    public bool spaceAfter;
    public override bool ShowGizmoOnPawn(Pawn pawn)
    {
        return pawn.Psycasts().previousUnlockedPaths.Contains(path) is false;
    }
    public bool PrereqsCompleted(Pawn pawn) => PrereqsCompleted(pawn.GetComp<CompAbilities>());

    public bool PrereqsCompleted(CompAbilities compAbilities)
    {
        return prerequisites.NullOrEmpty() || compAbilities.LearnedAbilities.Any(ab => prerequisites.Contains(ab.def));
    }

    public void UnlockWithPrereqs(CompAbilities compAbilities)
    {
        foreach (var prerequisite in prerequisites)
        {
            var extension = prerequisite.GetModExtension<AbilityExtension_Psycast>();
            if (extension != null)
                extension.UnlockWithPrereqs(compAbilities);
            else
                compAbilities.GiveAbility(prerequisite);
        }

        compAbilities.GiveAbility(abilityDef);
    }

    public float GetPsyfocusUsedByPawn(Pawn pawn) => psyfocusCost * pawn.GetStatValue(VPE_DefOf.VPE_PsyfocusCostFactor);

    public float GetEntropyUsedByPawn(Pawn pawn) =>
        entropyGainStatFactors.Aggregate(entropyGain, (current, statFactor) => current * (pawn.GetStatValue(statFactor.stat) * statFactor.value));

    public override bool IsEnabledForPawn(Ability ability, out string reason)
    {
        if (path.CanPawnUnlock(ability.pawn) is false)
        {
            if (path.ignoreLockRestrictionsForNeurotrainers is false)
            {
                reason = path.lockedReason;
                return false;
            }
        }

        var hediff = ability?.pawn?.Psycasts();
        if (hediff != null)
        {
            if (ability.pawn.psychicEntropy.PsychicSensitivity < float.Epsilon)
            {
                reason = "CommandPsycastZeroPsychicSensitivity".Translate();
                return false;
            }

            var psyfocusCost = GetPsyfocusUsedByPawn(ability.pawn);
            if (!hediff.SufficientPsyfocusPresent(psyfocusCost))
            {
                reason = "CommandPsycastNotEnoughPsyfocus".Translate(
                    psyfocusCost.ToStringPercent("#.0"), ability.pawn.psychicEntropy.CurrentPsyfocus.ToStringPercent("#.0"),
                    ability.def.label.Named("PSYCASTNAME"), ability.pawn.Named("CASTERNAME"));
                return false;
            }

            if (ability.pawn.psychicEntropy.WouldOverflowEntropy(GetEntropyUsedByPawn(ability.pawn)))
            {
                reason = "CommandPsycastWouldExceedEntropy".Translate(ability.def.label);
                return false;
            }

            if (hediff.CurrentlyChanneling != null)
            {
                reason = "VPE.CurrentChanneling".Translate(hediff.CurrentlyChanneling.def.LabelCap);
                return false;
            }

            if (ability.pawn.Downed)
            {
                reason = "IsIncapped".Translate(ability.pawn.LabelShort, ability.pawn);
                return false;
            }

            reason = string.Empty;
            return true;
        }

        reason = "VPE.NotPsycaster".Translate();
        return false;
    }

    public override void Cast(GlobalTargetInfo[] targets, Ability ability)
    {
        base.Cast(targets, ability);

        var psycastHediff =
            (Hediff_PsycastAbilities)ability.pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_PsycastAbilityImplant);
        psycastHediff.UseAbility(GetPsyfocusUsedByPawn(ability.pawn), GetEntropyUsedByPawn(ability.pawn));
        if (ability is IChannelledPsycast channelled) psycastHediff.BeginChannelling(channelled);
    }

    public override string GetDescription(Ability ability)
    {
        StringBuilder builder = new();

        var psyfocusCost = GetPsyfocusUsedByPawn(ability.pawn);
        if (psyfocusCost > float.Epsilon)
            builder.AppendInNewLine($"{"AbilityPsyfocusCost".Translate()}: {psyfocusCost.ToStringPercent()}");

        var entropy = GetEntropyUsedByPawn(ability.pawn);
        if (entropy > float.Epsilon)
            builder.AppendInNewLine($"{"AbilityEntropyGain".Translate()}: {entropy}");

        return builder.ToString().Colorize(Color.cyan);
    }

    public override void WarmupToil(Toil toil)
    {
        base.WarmupToil(toil);
        if (!showCastBubble) return;
        toil.AddPreInitAction(delegate
        {
            var mote = (MoteCastBubble)ThingMaker.MakeThing(VPE_DefOf.VPE_Mote_Cast);
            mote.Setup(toil.actor, toil.actor.GetComp<CompAbilities>().currentlyCasting);
            GenSpawn.Spawn(mote, toil.actor.Position, toil.actor.Map);
        });
    }

    public override void TargetingOnGUI(LocalTargetInfo target, Ability ability)
    {
        base.TargetingOnGUI(target, ability);
        if (!psychic) return;
        var validTargets = ability.currentTargets.Where(t => t.IsValid && t.Map != null).ToList();
        var targets = new GlobalTargetInfo[validTargets.Count + 1];
        validTargets.CopyTo(targets, 0);
        targets[targets.Length - 1] = target.ToGlobalTargetInfo(validTargets?.LastOrDefault().Map ?? ability.pawn.Map);
        ability.ModifyTargets(ref targets);
        foreach (var targetInfo in targets)
            if (targetInfo.Thing is Pawn p)
            {
                var sense = p.GetStatValue(StatDefOf.PsychicSensitivity);
                if (sense < 1.401298E-45f)
                {
                    var drawPos = p.DrawPos;
                    drawPos.z += 1f;
                    GenMapUI.DrawText(new Vector2(drawPos.x, drawPos.z), "Ineffective".Translate(), Color.red);
                }
                else
                {
                    var drawPos = p.DrawPos;
                    drawPos.z += 1f;
                    GenMapUI.DrawText(new Vector2(drawPos.x, drawPos.z), StatDefOf.PsychicSensitivity.LabelCap + ": " + sense.ToStringPercent(),
                        sense > float.Epsilon ? Color.white : Color.red);
                }
            }
    }

    public override bool Valid(GlobalTargetInfo[] targets, Ability ability, bool throwMessages = false)
    {
        var valid = base.Valid(targets, ability, throwMessages);
        if (valid)
        {
            string reason;
            valid = IsEnabledForPawn(ability, out reason);
            if (!valid && throwMessages) Messages.Message(reason, MessageTypeDefOf.RejectInput, false);
        }

        return valid;
    }

    public override bool ValidateTarget(LocalTargetInfo target, Ability ability, bool throwMessages = false)
    {
        if (psychic && target.Pawn is { } p && p.GetStatValue(StatDefOf.PsychicSensitivity) < 1.401298E-45f)
        {
            if (throwMessages) Messages.Message("Ineffective".Translate(), MessageTypeDefOf.RejectInput, false);
            return false;
        }

        return true;
    }
}

public static class AbilityExtensionPsycastUtility
{
    private static readonly Dictionary<AbilityDef, AbilityExtension_Psycast> cache = new();

    public static AbilityExtension_Psycast Psycast(this AbilityDef def)
    {
        if (cache.TryGetValue(def, out var ext)) return ext;
        ext = def.GetModExtension<AbilityExtension_Psycast>();
        cache[def] = ext;
        return ext;
    }
}
