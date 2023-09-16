using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using VanillaPsycastsExpanded.UI;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;
using AbilityDef = VFECore.Abilities.AbilityDef;

// ReSharper disable InconsistentNaming

namespace VanillaPsycastsExpanded;

[StaticConstructorOnStartup]
public class Hediff_PsycastAbilities : Hediff_Abilities
{
    private static readonly Texture2D PsySetNext = ContentFinder<Texture2D>.Get("UI/Gizmos/Psyset_Next");

    public float experience;

    public int maxLevelFromTitles;
    public int points;
    public List<PsycasterPathDef> previousUnlockedPaths = new();

    public Hediff_Psylink psylink;
    public List<PsySet> psysets = new();
    public List<MeditationFocusDef> unlockedMeditationFoci = new();
    public List<PsycasterPathDef> unlockedPaths = new();
    private IChannelledPsycast currentlyChanneling;


    private HediffStage curStage;
    private List<IMinHeatGiver> minHeatGivers = new();
    private int psysetIndex;

    private int statPoints;

    public Ability CurrentlyChanneling => currentlyChanneling as Ability;

    public override HediffStage CurStage
    {
        get
        {
            if (curStage == null) RecacheCurStage();
            return curStage;
        }
    }

    public IEnumerable<Gizmo> GetPsySetGizmos()
    {
        if (psysets.Count > 0)
        {
            var nextIndex = psysetIndex + 1;
            if (nextIndex > psysets.Count) nextIndex = 0;
            yield return new Command_ActionWithFloat
            {
                defaultLabel = "VPE.PsySetNext".Translate(),
                defaultDesc = "VPE.PsySetDesc".Translate(PsySetLabel(psysetIndex), PsySetLabel(nextIndex)),
                icon = PsySetNext,
                action = () => psysetIndex = nextIndex,
                Order = 10f,
                floatMenuGetter = GetPsySetFloatMenuOptions
            };
        }
    }

    private string PsySetLabel(int index)
    {
        if (index == psysets.Count) return "VPE.All".Translate();
        return psysets[index].Name;
    }

    private IEnumerable<FloatMenuOption> GetPsySetFloatMenuOptions()
    {
        for (var i = 0; i <= psysets.Count; i++)
        {
            var index = i;
            yield return new(PsySetLabel(index), () => psysetIndex = index);
        }
    }

    public void InitializeFromPsylink(Hediff_Psylink psylink)
    {
        this.psylink = psylink;
        level = psylink.level;
        points = level;
        if (level <= 1) points = 2;
        RecacheCurStage();
    }

    private void RecacheCurStage()
    {
        curStage = new()
        {
            statOffsets = new()
            {
                new() { stat = StatDefOf.PsychicEntropyMax, value = level * 5 + statPoints * 10 },
                new() { stat = StatDefOf.PsychicEntropyRecoveryRate, value = level * 0.0125f + statPoints * 0.05f },
                new() { stat = StatDefOf.PsychicSensitivity, value = statPoints * 0.05f },
                new() { stat = VPE_DefOf.VPE_PsyfocusCostFactor, value = statPoints * -0.01f },
                new() { stat = VPE_DefOf.VPE_PsychicEntropyMinimum, value = minHeatGivers.Sum(giver => giver.MinHeat) }
            },
            becomeVisible = false
        };
        if (PsycastsMod.Settings.changeFocusGain)
            curStage.statOffsets.Add(new()
            {
                stat = StatDefOf.MeditationFocusGain,
                value = statPoints * 0.1f
            });
        if (pawn.Spawned) pawn.health.Notify_HediffChanged(this);
    }

    public void UseAbility(float focus, float entropy)
    {
        pawn.psychicEntropy.TryAddEntropy(entropy);
        pawn.psychicEntropy.OffsetPsyfocusDirectly(-focus);
    }

    public void ChangeLevel(int levelOffset, bool sendLetter)
    {
        ChangeLevel(levelOffset);
        if (sendLetter && PawnUtility.ShouldSendNotificationAbout(pawn))
            Find.LetterStack.ReceiveLetter("VPE.PsylinkGained".Translate(pawn.LabelShortCap),
                "VPE.PsylinkGained.Desc".Translate(pawn.LabelShortCap,
                    pawn.gender.GetPronoun().CapitalizeFirst(),
                    ExperienceRequiredForLevel(level + 1)), LetterDefOf.PositiveEvent,
                pawn);
    }

    public override void ChangeLevel(int levelOffset)
    {
        base.ChangeLevel(levelOffset);
        points += levelOffset;
        RecacheCurStage();
        psylink ??= pawn.health.hediffSet.hediffs.OfType<Hediff_Psylink>().FirstOrDefault();
        if (psylink == null)
        {
            pawn.ChangePsylinkLevel(level, false);
            psylink = pawn.health.hediffSet.hediffs.OfType<Hediff_Psylink>().First();
        }

        psylink.level = level;
    }

    public void Reset()
    {
        points = level;
        unlockedPaths.Clear();
        unlockedMeditationFoci.Clear();
        statPoints = 0;
        pawn.GetComp<CompAbilities>()?.LearnedAbilities.RemoveAll(a => a.def.Psycast() != null);
        RecacheCurStage();
    }

    public void GainExperience(float experienceGain, bool sendLetter = true)
    {
        if (level >= PsycastsMod.Settings.maxLevel) return;
        experience += experienceGain;
        var newLevelWasGainedAlready = false;
        while (experience >= ExperienceRequiredForLevel(level + 1))
        {
            ChangeLevel(1, sendLetter && !newLevelWasGainedAlready);
            newLevelWasGainedAlready = true;
            experience -= ExperienceRequiredForLevel(level);
        }
    }

    public bool SufficientPsyfocusPresent(float focusRequired) => pawn.psychicEntropy.CurrentPsyfocus > focusRequired;

    public override bool SatisfiesConditionForAbility(AbilityDef abilityDef) =>
        base.SatisfiesConditionForAbility(abilityDef) ||
        abilityDef.requiredHediff?.minimumLevel <= psylink.level;

    public void AddMinHeatGiver(IMinHeatGiver giver)
    {
        if (!minHeatGivers.Contains(giver))
        {
            minHeatGivers.Add(giver);
            RecacheCurStage();
        }
    }

    public void BeginChannelling(IChannelledPsycast psycast)
    {
        currentlyChanneling = psycast;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref experience, nameof(experience));
        Scribe_Values.Look(ref points, nameof(points));
        Scribe_Values.Look(ref statPoints, nameof(statPoints));
        Scribe_Values.Look(ref psysetIndex, nameof(psysetIndex));
        Scribe_Values.Look(ref maxLevelFromTitles, nameof(maxLevelFromTitles));
        Scribe_Collections.Look(ref previousUnlockedPaths, nameof(previousUnlockedPaths), LookMode.Def);
        Scribe_Collections.Look(ref unlockedPaths, nameof(unlockedPaths), LookMode.Def);
        Scribe_Collections.Look(ref unlockedMeditationFoci, nameof(unlockedMeditationFoci), LookMode.Def);
        Scribe_Collections.Look(ref psysets, nameof(psysets), LookMode.Deep);
        Scribe_Collections.Look(ref minHeatGivers, nameof(minHeatGivers), LookMode.Reference);
        Scribe_References.Look(ref psylink, nameof(psylink));
        Scribe_References.Look(ref currentlyChanneling, nameof(currentlyChanneling));

        minHeatGivers ??= new();
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            unlockedPaths ??= new();
            previousUnlockedPaths ??= new();
            RecacheCurStage();
        }
    }

    public void SpentPoints(int count = 1)
    {
        points -= count;
    }

    public void ImproveStats(int count = 1)
    {
        statPoints += count;
        RecacheCurStage();
    }

    public void UnlockPath(PsycasterPathDef path)
    {
        unlockedPaths.Add(path);
    }

    public void UnlockMeditationFocus(MeditationFocusDef focus)
    {
        unlockedMeditationFoci.Add(focus);
        MeditationFocusTypeAvailabilityCache.ClearFor(pawn);
    }

    public bool ShouldShow(Ability ability) => psysetIndex == psysets.Count || psysets[psysetIndex].Abilities.Contains(ability.def);

    public void RemovePsySet(PsySet set)
    {
        psysets.Remove(set);
        psysetIndex = Mathf.Clamp(psysetIndex, 0, psysets.Count);
    }

    public static int ExperienceRequiredForLevel(int level) =>
        level switch
        {
            <= 1 => 100,
            <= 20 => Mathf.RoundToInt(ExperienceRequiredForLevel(level - 1) * 1.15f),
            <= 30 => Mathf.RoundToInt(ExperienceRequiredForLevel(level - 1) * 1.10f),
            _ => Mathf.RoundToInt(ExperienceRequiredForLevel(level - 1) * 1.05f)
        };

    public override void GiveRandomAbilityAtLevel(int? forLevel = null) { }

    public override void Tick()
    {
        base.Tick();
        if (currentlyChanneling is { IsActive: false }) currentlyChanneling = null;
        if (minHeatGivers.RemoveAll(giver => !giver.IsActive) > 0) RecacheCurStage();
    }
}
