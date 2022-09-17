namespace VanillaPsycastsExpanded;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UI;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;
using AbilityDef = VFECore.Abilities.AbilityDef;

[StaticConstructorOnStartup]
public class Hediff_PsycastAbilities : Hediff_Abilities
{
    private static readonly Texture2D PsySetNext = ContentFinder<Texture2D>.Get("UI/Gizmos/Psyset_Next");

    public  float              experience;
    public  int                points;
    private IChannelledPsycast currentlyChanneling;

    private int statPoints;
    private int psysetIndex;

    public  Hediff_Psylink           psylink;
    public  List<PsySet>             psysets                = new();
    public  List<MeditationFocusDef> unlockedMeditationFoci = new();
    public  List<PsycasterPathDef>   unlockedPaths          = new();
    private List<IMinHeatGiver>      minHeatGivers          = new();

    public int maxLevelFromTitles;


    private HediffStage curStage;

    public Ability CurrentlyChanneling => this.currentlyChanneling as Ability;

    public override HediffStage CurStage
    {
        get
        {
            if (this.curStage == null) this.RecacheCurStage();
            return this.curStage;
        }
    }

    public IEnumerable<Gizmo> GetPsySetGizmos()
    {
        if (this.psysets.Count > 0)
        {
            int nextIndex                                 = this.psysetIndex + 1;
            if (nextIndex > this.psysets.Count) nextIndex = 0;
            yield return new Command_ActionWithFloat
            {
                defaultLabel    = "VPE.PsySetNext".Translate(),
                defaultDesc     = "VPE.PsySetDesc".Translate(this.PsySetLabel(this.psysetIndex), this.PsySetLabel(nextIndex)),
                icon            = PsySetNext,
                action          = () => this.psysetIndex = nextIndex,
                order           = 10f,
                floatMenuGetter = this.GetPsySetFloatMenuOptions
            };
        }
    }

    private string PsySetLabel(int index)
    {
        if (index == this.psysets.Count) return "VPE.All".Translate();
        return this.psysets[index].Name;
    }

    private IEnumerable<FloatMenuOption> GetPsySetFloatMenuOptions()
    {
        for (int i = 0; i <= this.psysets.Count; i++)
        {
            int index = i;
            yield return new FloatMenuOption(this.PsySetLabel(index), () => this.psysetIndex = index);
        }
    }

    public void InitializeFromPsylink(Hediff_Psylink psylink)
    {
        this.psylink = psylink;
        this.level   = psylink.level;
        this.points  = this.level;
        this.RecacheCurStage();
        if (!this.unlockedPaths.Any())
            this.unlockedPaths.Add(DefDatabase<PsycasterPathDef>.AllDefs.Where(path => path.CanPawnUnlock(psylink.pawn)).RandomElement());
    }

    private void RecacheCurStage()
    {
        this.curStage = new HediffStage
        {
            statOffsets = new List<StatModifier>
            {
                new() { stat = StatDefOf.PsychicEntropyMax, value          = this.level * 5 + this.statPoints * 20 },
                new() { stat = StatDefOf.PsychicEntropyRecoveryRate, value = this.statPoints * 0.2f },
                new() { stat = StatDefOf.PsychicSensitivity, value         = this.statPoints * 0.05f },
                new() { stat = StatDefOf.MeditationFocusGain, value        = this.statPoints * 0.1f },
                new() { stat = VPE_DefOf.VPE_PsyfocusCostFactor, value     = this.statPoints * -0.01f },
                new() { stat = VPE_DefOf.VPE_PsychicEntropyMinimum, value  = this.minHeatGivers.Sum(giver => giver.MinHeat) }
            },
            becomeVisible = false
        };
        this.pawn.health.Notify_HediffChanged(this);
    }

    public void UseAbility(float focus, float entropy)
    {
        this.pawn.psychicEntropy.TryAddEntropy(entropy);
        this.pawn.psychicEntropy.OffsetPsyfocusDirectly(-focus);
    }

    public void ChangeLevel(int levelOffset, bool sendLetter)
    {
        this.ChangeLevel(levelOffset);
        if (sendLetter && PawnUtility.ShouldSendNotificationAbout(this.pawn))
            Find.LetterStack.ReceiveLetter("VPE.PsylinkGained".Translate(this.pawn.LabelShortCap),
                                           "VPE.PsylinkGained.Desc".Translate(this.pawn.LabelShortCap,
                                                                              this.pawn.gender.GetPronoun().CapitalizeFirst(),
                                                                              ExperienceRequiredForLevel(this.level + 1)), LetterDefOf.PositiveEvent,
                                           this.pawn);
    }

    public override void ChangeLevel(int levelOffset)
    {
        base.ChangeLevel(levelOffset);
        this.points += levelOffset;
        this.RecacheCurStage();
        this.psylink ??= this.pawn.health.hediffSet.hediffs.OfType<Hediff_Psylink>().FirstOrDefault();
        if (this.psylink == null)
        {
            this.pawn.ChangePsylinkLevel(this.level, false);
            this.psylink = this.pawn.health.hediffSet.hediffs.OfType<Hediff_Psylink>().First();
        }

        this.psylink.level = this.level;
    }

    public void Reset()
    {
        this.points = this.level;
        this.unlockedPaths.Clear();
        this.unlockedMeditationFoci.Clear();
        this.statPoints = 0;
        this.pawn.GetComp<CompAbilities>()?.LearnedAbilities.RemoveAll(a => a.def.Psycast() != null);
        this.RecacheCurStage();
    }

    public void GainExperience(float experienceGain, bool sendLetter = true)
    {
        this.experience += experienceGain;
        bool newLevelWasGainedAlready = false;
        while (this.experience >= ExperienceRequiredForLevel(this.level + 1))
        {
            this.ChangeLevel(1, sendLetter && !newLevelWasGainedAlready);
            newLevelWasGainedAlready =  true;
            this.experience          -= ExperienceRequiredForLevel(this.level);
        }
    }

    public bool SufficientPsyfocusPresent(float focusRequired) =>
        this.pawn.psychicEntropy.CurrentPsyfocus > focusRequired;

    public override bool SatisfiesConditionForAbility(AbilityDef abilityDef) =>
        base.SatisfiesConditionForAbility(abilityDef) ||
        abilityDef.requiredHediff?.minimumLevel <= this.psylink.level;

    public void AddMinHeatGiver(IMinHeatGiver giver)
    {
        if (!this.minHeatGivers.Contains(giver))
        {
            this.minHeatGivers.Add(giver);
            this.RecacheCurStage();
        }
    }

    public void BeginChannelling(IChannelledPsycast psycast)
    {
        this.currentlyChanneling = psycast;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref this.experience,         nameof(this.experience));
        Scribe_Values.Look(ref this.points,             nameof(this.points));
        Scribe_Values.Look(ref this.statPoints,         nameof(this.statPoints));
        Scribe_Values.Look(ref this.psysetIndex,        nameof(this.psysetIndex));
        Scribe_Values.Look(ref this.maxLevelFromTitles, nameof(this.maxLevelFromTitles));
        Scribe_Collections.Look(ref this.unlockedPaths,          nameof(this.unlockedPaths),          LookMode.Def);
        Scribe_Collections.Look(ref this.unlockedMeditationFoci, nameof(this.unlockedMeditationFoci), LookMode.Def);
        Scribe_Collections.Look(ref this.psysets,                nameof(this.psysets),                LookMode.Deep);
        Scribe_Collections.Look(ref this.minHeatGivers,          nameof(this.minHeatGivers),          LookMode.Reference);
        Scribe_References.Look(ref this.psylink,             nameof(this.psylink));
        Scribe_References.Look(ref this.currentlyChanneling, nameof(this.currentlyChanneling));

        this.minHeatGivers ??= new List<IMinHeatGiver>();
        if (Scribe.mode == LoadSaveMode.PostLoadInit) this.RecacheCurStage();
    }

    public void SpentPoints(int count = 1)
    {
        this.points -= count;
    }

    public void ImproveStats(int count = 1)
    {
        this.statPoints += count;
        this.RecacheCurStage();
    }

    public void UnlockPath(PsycasterPathDef path)
    {
        this.unlockedPaths.Add(path);
    }

    public void UnlockMeditationFocus(MeditationFocusDef focus)
    {
        this.unlockedMeditationFoci.Add(focus);
        MeditationFocusTypeAvailabilityCache.ClearFor(this.pawn);
    }

    public bool ShouldShow(Ability ability) => this.psysetIndex == this.psysets.Count || this.psysets[this.psysetIndex].Abilities.Contains(ability.def);

    public void RemovePsySet(PsySet set)
    {
        this.psysets.Remove(set);
        this.psysetIndex = Mathf.Clamp(this.psysetIndex, 0, this.psysets.Count);
    }

    public static int ExperienceRequiredForLevel(int level) =>
        level switch
        {
            <= 1  => 100,
            <= 20 => Mathf.RoundToInt(ExperienceRequiredForLevel(level - 1) * 1.15f),
            <= 30 => Mathf.RoundToInt(ExperienceRequiredForLevel(level - 1) * 1.10f),
            _     => Mathf.RoundToInt(ExperienceRequiredForLevel(level - 1) * 1.05f)
        };

    public override void GiveRandomAbilityAtLevel(int? forLevel = null)
    {
    }

    public override void Tick()
    {
        base.Tick();
        if (this.currentlyChanneling is { IsActive: false }) this.currentlyChanneling = null;
        if (this.minHeatGivers.RemoveAll(giver => !giver.IsActive) > 0) this.RecacheCurStage();
    }
}