namespace VanillaPsycastsExpanded;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using AbilityDef = VFECore.Abilities.AbilityDef;

public class PsycasterPathDef : Def
{
    public static AbilityDef Blank;
    public static int        TotalPoints;

    public List<BackstoryCategoryAndSlot> requiredBackstoriesAny;

    public string  background;
    public string  altBackground;
    public Color   backgroundColor;
    public int     width;
    public int     height;
    public int     order;
    public string  tab;
    public string  tooltip;
    public MemeDef requiredMeme;

    [MustTranslate] public string lockedReason;

    [Unsaved] public Texture2D        backgroundImage;
    [Unsaved] public Texture2D        altBackgroundImage;
    [Unsaved] public Texture2D        backgroundImageLocked;
    [Unsaved] public Texture2D        altBackgroundImageLocked;
    [Unsaved] public bool             HasAbilities;
    [Unsaved] public int              MaxLevel;
    [Unsaved] public List<AbilityDef> abilities;
    [Unsaved] public AbilityDef[][]   abilityLevelsInOrder;

    public virtual bool CanPawnUnlock(Pawn pawn) => this.PawnHasCorrectBackstory(pawn) && this.PawnHasMeme(pawn);

    private bool PawnHasMeme(Pawn pawn) => this.requiredMeme == null || (pawn.Ideo?.memes.Contains(this.requiredMeme) ?? false);

    private bool PawnHasCorrectBackstory(Pawn pawn)
    {
        if (this.requiredBackstoriesAny.NullOrEmpty()) return true;
        foreach (BackstoryCategoryAndSlot requirement in this.requiredBackstoriesAny)
        {
            List<string> list6 = requirement.slot == BackstorySlot.Adulthood
                ? pawn.story.adulthood?.spawnCategories
                : pawn.story.childhood?.spawnCategories;
            if (list6 != null && list6.Contains(requirement.categoryName)) return true;
        }

        return false;
    }

    public override void PostLoad()
    {
        base.PostLoad();
        LongEventHandler.ExecuteWhenFinished(delegate
        {
            if (!this.background.NullOrEmpty()) this.backgroundImage       = ContentFinder<Texture2D>.Get(this.background);
            if (!this.altBackground.NullOrEmpty()) this.altBackgroundImage = ContentFinder<Texture2D>.Get(this.altBackground);

            if (this.width > 0 && this.height > 0)
            {
                Texture2D tex    = new(this.width, this.height);
                Color[]   colors = new Color[this.width * this.height];

                for (int i = 0; i < colors.Length; i++) colors[i] = this.backgroundColor;

                tex.SetPixels(colors);
                tex.Apply();

                if (this.backgroundImage    == null) this.backgroundImage    = tex;
                if (this.altBackgroundImage == null) this.altBackgroundImage = tex;
            }

            if (this.backgroundImage    == null && this.altBackgroundImage != null) this.backgroundImage    = this.altBackgroundImage;
            if (this.altBackgroundImage == null && this.backgroundImage    != null) this.altBackgroundImage = this.backgroundImage;
        });
    }

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        Blank       ??= new AbilityDef();
        TotalPoints +=  1;

        this.abilities = new List<AbilityDef>();
        foreach (AbilityDef abilityDef in DefDatabase<AbilityDef>.AllDefsListForReading)
        {
            AbilityExtension_Psycast psycast = abilityDef.GetModExtension<AbilityExtension_Psycast>();
            if (psycast is not null && psycast.path == this) this.abilities.Add(abilityDef);
        }

        this.MaxLevel =  this.abilities.Max(ab => ab.Psycast().level);
        TotalPoints   += this.abilities.Count;

        this.abilityLevelsInOrder = new AbilityDef[this.MaxLevel][];
        foreach (IGrouping<int, AbilityDef> abilityDefs in this.abilities.GroupBy(ab => ab.Psycast().level))
            this.abilityLevelsInOrder[abilityDefs.Key - 1] = abilityDefs.OrderBy(ab => ab.Psycast().order)
                                                                        .SelectMany(ab => ab.Psycast().spaceAfter
                                                                                        ? new List<AbilityDef> { ab, Blank }
                                                                                        : Gen.YieldSingle(ab)).ToArray();

        this.HasAbilities = this.abilityLevelsInOrder.Any(arr => !arr.NullOrEmpty());
        if (!this.HasAbilities) return;
        // Log.Message($"Abilities for {this.label}:");
        for (int i = 0; i < this.abilityLevelsInOrder.Length; i++)
        {
            AbilityDef[] arr = this.abilityLevelsInOrder[i];
            if (arr is null)
                this.abilityLevelsInOrder[i] = new AbilityDef[0];
            //     else
            //     {
            //         Log.Message($"  Level {i + 1}:");
            //         for (int j = 0; j < arr.Length; j++) Log.Message($"    {j}: {arr[j].label}");
            //     }
        }
    }
}