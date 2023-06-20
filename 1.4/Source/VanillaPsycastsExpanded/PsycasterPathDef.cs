using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using AbilityDef = VFECore.Abilities.AbilityDef;

// ReSharper disable InconsistentNaming

namespace VanillaPsycastsExpanded;

public class PsycasterPathDef : Def
{
    public static AbilityDef Blank;
    public static int TotalPoints;
    [Unsaved] public List<AbilityDef> abilities;
    [Unsaved] public AbilityDef[][] abilityLevelsInOrder;
    public string altBackground;
    [Unsaved] public Texture2D altBackgroundImage;

    public string background;
    public Color backgroundColor;

    [Unsaved] public Texture2D backgroundImage;
    [Unsaved] public bool HasAbilities;
    public int height;

    [MustTranslate] public string lockedReason;
    [Unsaved] public int MaxLevel;
    public int order;

    public List<BackstoryCategoryAndSlot> requiredBackstoriesAny;
    public MeditationFocusDef requiredFocus;
    public GeneDef requiredGene;
    public MemeDef requiredMeme;
    public bool requiredMechanitor = false;
    public bool ignoreLockRestrictionsForNeurotrainers = true;
    public bool ensureLockRequirement;
    public string tab;
    public string tooltip;
    public int width;

    public virtual bool CanPawnUnlock(Pawn pawn) => PawnHasCorrectBackstory(pawn) && PawnHasMeme(pawn) && PawnHasGene(pawn) && PawnIsMechanitor(pawn) && PawnHasCorrectFocus(pawn);

    private bool PawnHasMeme(Pawn pawn) => requiredMeme == null || (pawn.Ideo?.memes.Contains(requiredMeme) ?? false);
    private bool PawnHasGene(Pawn pawn) => requiredGene == null || (pawn.genes?.GetGene(requiredGene)?.Active ?? false);
    private bool PawnIsMechanitor(Pawn pawn) => !requiredMechanitor || (MechanitorUtility.IsMechanitor(pawn));
    private bool PawnHasCorrectFocus(Pawn pawn) => requiredFocus == null || requiredFocus.CanPawnUse(pawn);

    private bool PawnHasCorrectBackstory(Pawn pawn)
    {
        if (requiredBackstoriesAny.NullOrEmpty()) return true;
        foreach (var requirement in requiredBackstoriesAny)
            if (pawn.story.GetBackstory(requirement.slot)?.spawnCategories is { } spawnCategories && spawnCategories.Contains(requirement.categoryName))
                return true;

        return false;
    }


    public override void PostLoad()
    {
        base.PostLoad();
        LongEventHandler.ExecuteWhenFinished(delegate
        {
            if (!background.NullOrEmpty()) backgroundImage = ContentFinder<Texture2D>.Get(background);
            if (!altBackground.NullOrEmpty()) altBackgroundImage = ContentFinder<Texture2D>.Get(altBackground);

            if (width > 0 && height > 0)
            {
                Texture2D tex = new(width, height);
                var colors = new Color[width * height];

                for (var i = 0; i < colors.Length; i++) colors[i] = backgroundColor;

                tex.SetPixels(colors);
                tex.Apply();

                if (backgroundImage == null) backgroundImage = tex;
                if (altBackgroundImage == null) altBackgroundImage = tex;
            }

            if (backgroundImage == null && altBackgroundImage != null) backgroundImage = altBackgroundImage;
            if (altBackgroundImage == null && backgroundImage != null) altBackgroundImage = backgroundImage;
        });
    }

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        Blank ??= new AbilityDef();
        TotalPoints += 1;

        abilities = new List<AbilityDef>();
        foreach (var abilityDef in DefDatabase<AbilityDef>.AllDefsListForReading)
        {
            var psycast = abilityDef.GetModExtension<AbilityExtension_Psycast>();
            if (psycast is not null && psycast.path == this) abilities.Add(abilityDef);
        }

        MaxLevel = abilities.Max(ab => ab.Psycast().level);
        TotalPoints += abilities.Count;

        abilityLevelsInOrder = new AbilityDef[MaxLevel][];
        foreach (var abilityDefs in abilities.GroupBy(ab => ab.Psycast().level))
            abilityLevelsInOrder[abilityDefs.Key - 1] = abilityDefs.OrderBy(ab => ab.Psycast().order)
               .SelectMany(ab => ab.Psycast().spaceAfter
                    ? new List<AbilityDef> { ab, Blank }
                    : Gen.YieldSingle(ab))
               .ToArray();

        HasAbilities = abilityLevelsInOrder.Any(arr => !arr.NullOrEmpty());
        if (!HasAbilities) return;
        // Log.Message($"Abilities for {this.label}:");
        for (var i = 0; i < abilityLevelsInOrder.Length; i++)
        {
            var arr = abilityLevelsInOrder[i];
            if (arr is null)
                abilityLevelsInOrder[i] = new AbilityDef[0];
            //     else
            //     {
            //         Log.Message($"  Level {i + 1}:");
            //         for (int j = 0; j < arr.Length; j++) Log.Message($"    {j}: {arr[j].label}");
            //     }
        }
    }
}
