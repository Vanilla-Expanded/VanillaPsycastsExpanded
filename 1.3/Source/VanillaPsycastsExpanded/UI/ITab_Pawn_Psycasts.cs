namespace VanillaPsycastsExpanded.UI;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using VFECore.UItils;
using AbilityDef = VFECore.Abilities.AbilityDef;

[StaticConstructorOnStartup]
public class ITab_Pawn_Psycasts : ITab
{
    private readonly List<MeditationFocusDef>                   foci;
    private readonly Dictionary<string, List<PsycasterPathDef>> pathsByTab;
    private readonly List<TabRecord>                            tabs;
    private readonly Dictionary<AbilityDef, Vector2>            abilityPos = new();

    private Hediff_PsycastAbilities hediff;
    private CompAbilities           compAbilities;

    private string  curTab;
    private float   lastPathsHeight;
    private int     pathsPerRow;
    private Vector2 pathsScrollPos;
    private Pawn    pawn;
    private Vector2 psysetsScrollPos;
    private bool    useAltBackgrounds;
    private bool    devMode;
    private float   psysetSectionHeight;
    private bool    smallMode;

    static ITab_Pawn_Psycasts()
    {
        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            if (def.race is { Humanlike: true })
            {
                def.inspectorTabs?.Add(typeof(ITab_Pawn_Psycasts));
                def.inspectorTabsResolved?.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Psycasts)));
            }
    }

    public ITab_Pawn_Psycasts()
    {
        this.labelKey   = "VPE.Psycasts";
        this.size       = new Vector2(UI.screenWidth, UI.screenHeight * 0.75f);
        this.pathsByTab = DefDatabase<PsycasterPathDef>.AllDefs.GroupBy(def => def.tab).ToDictionary(group => group.Key, group => group.ToList());
        this.foci = DefDatabase<MeditationFocusDef>.AllDefs.OrderByDescending(def => def.modContentPack.IsOfficialMod).ThenByDescending(def => def.label)
                                                   .ToList();
        this.tabs   = this.pathsByTab.Select(kv => new TabRecord(kv.Key, () => this.curTab = kv.Key, () => this.curTab == kv.Key)).ToList();
        this.curTab = this.pathsByTab.Keys.FirstOrDefault();
    }

    public Vector2 Size                   => this.size;
    public float   RequestedPsysetsHeight { get; private set; }

    public override bool IsVisible =>
        Find.Selector.SingleSelectedThing is Pawn pawn && pawn.health.hediffSet.HasHediff(VPE_DefOf.VPE_PsycastAbilityImplant) &&
        pawn.Faction is { IsPlayer: true };

    protected override void UpdateSize()
    {
        base.UpdateSize();
        this.size.y      = this.PaneTopY - 30f;
        this.pathsPerRow = Mathf.FloorToInt(this.size.x * 0.67f / 200f);
        this.smallMode = PsycastsMod.Settings.smallMode switch
        {
            MultiCheckboxState.On  => true,
            MultiCheckboxState.Off => false,
            _                      => this.size.y <= 1080f / Prefs.UIScale
        };
    }

    public override void OnOpen()
    {
        base.OnOpen();
        this.pawn = (Pawn)Find.Selector.SingleSelectedThing;
        this.InitCache();
    }

    private void InitCache()
    {
        PsycastsUIUtility.Hediff        = this.hediff        = this.pawn.Psycasts();
        PsycastsUIUtility.CompAbilities = this.compAbilities = this.pawn.GetComp<CompAbilities>();
        this.abilityPos.Clear();
    }

    protected override void CloseTab()
    {
        base.CloseTab();
        this.pawn                       = null;
        PsycastsUIUtility.Hediff        = this.hediff        = null;
        PsycastsUIUtility.CompAbilities = this.compAbilities = null;
        this.abilityPos.Clear();
    }

    protected override void FillTab()
    {
        if (Find.Selector.SingleSelectedThing is Pawn p && this.pawn != p)
        {
            this.pawn = p;
            this.InitCache();
        }

        if (this.devMode && !Prefs.DevMode) this.devMode = false;

        if (this.pawn == null || this.hediff == null || this.compAbilities == null) return;
        GameFont         font         = Text.Font;
        TextAnchor       anchor       = Text.Anchor;
        Rect             tabRect      = new(Vector2.one                  * 20f, this.size - Vector2.one * 40f);
        Rect             pawnAndStats = tabRect.TakeLeftPart(this.size.x * 0.3f);
        Rect             paths        = tabRect.ContractedBy(5f);
        Listing_Standard listing      = new();
        listing.Begin(pawnAndStats);
        Text.Font = GameFont.Medium;
        listing.Label(this.pawn.Name.ToStringFull);
        listing.Label("VPE.PsyLevel".Translate(this.hediff.level));
        listing.Gap(10f);
        Rect bar = listing.GetRect(60f).ContractedBy(10f, 0f);
        Text.Anchor = TextAnchor.MiddleCenter;
        int xpForNext = Hediff_PsycastAbilities.ExperienceRequiredForLevel(this.hediff.level + 1);
        if (this.devMode)
        {
            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(bar.TakeRightPart(80f), "Dev: Level up"))
                this.hediff.GainExperience(xpForNext, false);
            Text.Font = GameFont.Medium;
        }

        Widgets.FillableBar(bar, this.hediff.experience / xpForNext);
        Widgets.Label(bar, $"{this.hediff.experience.ToStringByStyle(ToStringStyle.FloatOne)} / {xpForNext}");
        Text.Font = GameFont.Tiny;
        listing.Label("VPE.EarnXP".Translate());
        listing.Gap(10f);
        Text.Font   = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        listing.Label("VPE.Points".Translate(this.hediff.points));
        Text.Font = GameFont.Tiny;
        listing.Label("VPE.SpendPoints".Translate());
        listing.Gap(3f);
        Text.Anchor = TextAnchor.MiddleLeft;
        Text.Font   = GameFont.Small;
        float heightBefore = listing.CurHeight;
        if (listing.ButtonTextLabeled("VPE.PsycasterStats".Translate() + (this.smallMode ? $" ({"VPE.Hover".Translate()})" : ""), "VPE.Upgrade".Translate()))
        {
            int num = GenUI.CurrentAdjustmentMultiplier();
            if (this.devMode) this.hediff.ImproveStats(num);
            else if (this.hediff.points >= num)
            {
                this.hediff.SpentPoints(num);
                this.hediff.ImproveStats(num);
            }
            else Messages.Message("VPE.NotEnoughPoints".Translate(), MessageTypeDefOf.RejectInput, false);
        }

        float heightAfter = listing.CurHeight;
        if (this.smallMode)
        {
            Rect rect = new(pawnAndStats.x, heightBefore, pawnAndStats.width / 2f, heightAfter - heightBefore);
            if (Mouse.IsOver(rect))
            {
                Vector2 size = new(pawnAndStats.width, 150f);
                Find.WindowStack.ImmediateWindow(145 * 62346, new Rect(GenUI.GetMouseAttachedWindowPos(size.x, size.y), size), WindowLayer.Super,
                                                 delegate
                                                 {
                                                     Listing_Standard inner = new();
                                                     inner.Begin(new Rect(Vector2.one * 5f, size));
                                                     inner.StatDisplay(TexPsycasts.IconNeuralHeatLimit,     StatDefOf.PsychicEntropyMax,          this.pawn);
                                                     inner.StatDisplay(TexPsycasts.IconNeuralHeatRegenRate, StatDefOf.PsychicEntropyRecoveryRate, this.pawn);
                                                     inner.StatDisplay(TexPsycasts.IconPsychicSensitivity,  StatDefOf.PsychicSensitivity,         this.pawn);
                                                     inner.StatDisplay(TexPsycasts.IconPsyfocusGain,        StatDefOf.MeditationFocusGain,        this.pawn);
                                                     inner.StatDisplay(TexPsycasts.IconPsyfocusCost,        VPE_DefOf.VPE_PsyfocusCostFactor,     this.pawn);
                                                     inner.End();
                                                 });
            }
        }
        else
        {
            listing.StatDisplay(TexPsycasts.IconNeuralHeatLimit,     StatDefOf.PsychicEntropyMax,          this.pawn);
            listing.StatDisplay(TexPsycasts.IconNeuralHeatRegenRate, StatDefOf.PsychicEntropyRecoveryRate, this.pawn);
            listing.StatDisplay(TexPsycasts.IconPsychicSensitivity,  StatDefOf.PsychicSensitivity,         this.pawn);
            listing.StatDisplay(TexPsycasts.IconPsyfocusGain,        StatDefOf.MeditationFocusGain,        this.pawn);
            listing.StatDisplay(TexPsycasts.IconPsyfocusCost,        VPE_DefOf.VPE_PsyfocusCostFactor,     this.pawn);
        }

        listing.LabelWithIcon(TexPsycasts.IconFocusTypes, "VPE.FocusTypes".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        Rect  fociRect = listing.GetRect(48f);
        float x        = pawnAndStats.x;
        foreach (MeditationFocusDef def in this.foci)
        {
            if (x + 50f >= pawnAndStats.width)
            {
                x = pawnAndStats.x;
                listing.Gap(3f);
                fociRect = listing.GetRect(48f);
            }

            Rect rect = new(x, fociRect.y, 48f, 48f);
            this.DoFocus(rect, def);
            x += 50f;
        }

        listing.Gap(10f);
        if (this.smallMode)
        {
            if (listing.ButtonTextLabeled("VPE.PsysetCustomize".Translate(), "VPE.Edit".Translate())) Find.WindowStack.Add(new Dialog_EditPsysets(this));
        }
        else listing.Label("VPE.PsysetCustomize".Translate());

        Text.Font = GameFont.Tiny;
        listing.Label("VPE.PsysetDesc".Translate());
        float prePsysetHeight = listing.CurHeight;
        Rect  viewRect;
        if (!this.smallMode)
        {
            Rect psysets = listing.GetRect(this.psysetSectionHeight);
            Widgets.DrawMenuSection(psysets);
            viewRect = new Rect(0, 0, psysets.width - 20f, this.RequestedPsysetsHeight);
            Widgets.BeginScrollView(psysets.ContractedBy(3f, 6f), ref this.psysetsScrollPos, viewRect);
            this.DoPsysets(viewRect);
            Widgets.EndScrollView();
        }

        float postPsysetHeight = listing.CurHeight;
        listing.CheckboxLabeled("VPE.UseAltBackground".Translate(), ref this.useAltBackgrounds);
        if (Prefs.DevMode) listing.CheckboxLabeled("VPE.DevMode".Translate(), ref this.devMode);
        this.psysetSectionHeight = pawnAndStats.height - prePsysetHeight - (listing.CurHeight - postPsysetHeight);
        listing.End();
        if (this.pathsByTab.NullOrEmpty())
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font   = GameFont.Medium;
            Widgets.DrawMenuSection(paths);
            Widgets.Label(paths, "No Paths");
        }
        else
        {
            TabDrawer.DrawTabs(new Rect(paths.x, paths.y + 40f, paths.width, paths.height), this.tabs);
            paths.yMin += 40f;
            Widgets.DrawMenuSection(paths);
            viewRect = new Rect(0, 0, paths.width - 20f, this.lastPathsHeight);
            Widgets.BeginScrollView(paths, ref this.pathsScrollPos, viewRect);
            this.DoPaths(viewRect);
            Widgets.EndScrollView();
        }

        Text.Font   = font;
        Text.Anchor = anchor;
    }

    private void DoFocus(Rect inRect, MeditationFocusDef def)
    {
        Widgets.DrawBox(inRect, 3, Texture2D.grayTexture);
        bool unlocked  = def.CanPawnUse(this.pawn);
        bool canUnlock = def.CanUnlock(this.pawn, out string lockedReason);
        GUI.color = unlocked ? Color.white : Color.gray;
        GUI.DrawTexture(inRect.ContractedBy(5f), def.Icon());
        GUI.color = Color.white;
        TooltipHandler.TipRegion(inRect, def.LabelCap    + (def.description.NullOrEmpty() ? "" : "\n\n") +
                                         def.description + (canUnlock ? "" : $"\n\n{lockedReason}"));
        Widgets.DrawHighlightIfMouseover(inRect);
        if ((this.hediff.points >= 1 || this.devMode) && !unlocked && (canUnlock || this.devMode))
            if (Widgets.ButtonText(new Rect(inRect.xMax - 13f, inRect.yMax - 13f, 12f, 12f), "▲"))
            {
                if (!this.devMode) this.hediff.SpentPoints();
                this.hediff.UnlockMeditationFocus(def);
            }
    }

    public void DoPsysets(Rect inRect)
    {
        Listing_Standard listing = new();
        listing.Begin(inRect);
        foreach (PsySet psyset in this.hediff.psysets.ToList())
        {
            Rect rect = listing.GetRect(30f);
            Widgets.Label(rect.LeftHalf().LeftHalf(), psyset.Name);
            if (Widgets.ButtonText(rect.LeftHalf().RightHalf(), "VPE.Rename".Translate()))
                Find.WindowStack.Add(new Dialog_RenamePsyset(psyset));
            if (Widgets.ButtonText(rect.RightHalf().LeftHalf(), "VPE.Edit".Translate()))
                Find.WindowStack.Add(new Dialog_Psyset(psyset, this.pawn));
            if (Widgets.ButtonText(rect.RightHalf().RightHalf(), "VPE.Remove".Translate())) this.hediff.RemovePsySet(psyset);
        }

        if (Widgets.ButtonText(listing.GetRect(70f).LeftHalf().ContractedBy(5f), "VPE.CreatePsyset".Translate()))
        {
            PsySet psyset = new() { Name = "VPE.Untitled".Translate() };
            this.hediff.psysets.Add(psyset);
            Find.WindowStack.Add(new Dialog_Psyset(psyset, this.pawn));
        }

        this.RequestedPsysetsHeight = listing.CurHeight + 70f;
        listing.End();
    }

    private void DoPaths(Rect inRect)
    {
        Vector2 curPos       = inRect.position + Vector2.one * 10f;
        float   widthPerPath = (inRect.width - (this.pathsPerRow + 1) * 10f) / this.pathsPerRow;
        float   maxHeight    = 0f;
        int     paths        = this.pathsPerRow;
        foreach (PsycasterPathDef def in this.pathsByTab[this.curTab].OrderByDescending(path => this.hediff.unlockedPaths.Contains(path))
                                             .ThenBy(path => path.order).ThenBy(path => path.label))
        {
            Texture2D texture = this.useAltBackgrounds ? def.backgroundImage : def.altBackgroundImage;
            float     height  = widthPerPath / texture.width * texture.height + 30f;
            Rect      rect    = new(curPos, new Vector2(widthPerPath, height));
            PsycastsUIUtility.DrawPathBackground(ref rect, def, this.useAltBackgrounds);
            if (this.hediff.unlockedPaths.Contains(def))
            {
                if (def.HasAbilities) PsycastsUIUtility.DoPathAbilities(rect, def, this.abilityPos, this.DoAbility);
            }
            else
            {
                Widgets.DrawRectFast(rect, new Color(0f, 0f, 0f, this.useAltBackgrounds ? 0.7f : 0.55f));
                if (this.hediff.points >= 1 || this.devMode)
                {
                    Rect centerRect = rect.CenterRect(new Vector2(140f, 30f));
                    if (this.devMode || def.CanPawnUnlock(this.pawn))
                    {
                        if (Widgets.ButtonText(centerRect, "VPE.Unlock".Translate()))
                        {
                            if (!this.devMode) this.hediff.SpentPoints();
                            this.hediff.UnlockPath(def);
                        }
                    }
                    else
                    {
                        GUI.color = Color.grey;
                        string label = "VPE.Locked".Translate().Resolve() + ": " + def.lockedReason;
                        centerRect.width = Mathf.Max(centerRect.width, Text.CalcSize(label).x + 10f);
                        Widgets.ButtonText(centerRect, label, active: false);
                        GUI.color = Color.white;
                    }
                }

                TooltipHandler.TipRegion(
                    rect,
                    () => def.tooltip + "\n\n" + "VPE.AbilitiesList".Translate() + "\n" + def.abilities.Select(ab => ab.label).ToLineList("  ", true),
                    def.GetHashCode());
            }

            maxHeight =  Mathf.Max(maxHeight, height + 10f);
            curPos.x  += widthPerPath + 10f;
            paths--;
            if (paths == 0)
            {
                curPos.x  =  inRect.x + 10f;
                curPos.y  += maxHeight;
                paths     =  this.pathsPerRow;
                maxHeight =  0f;
            }
        }

        this.lastPathsHeight = curPos.y + maxHeight;
    }

    private void DoAbility(Rect inRect, AbilityDef ability)
    {
        bool unlockable = false;
        bool locked     = false;
        if (!this.compAbilities.HasAbility(ability))
        {
            if (this.devMode || (ability.Psycast().PrereqsCompleted(this.compAbilities) && this.hediff.points >= 1))
                unlockable = true;
            else locked    = true;
        }

        if (unlockable) QuickSearchWidget.DrawStrongHighlight(inRect.ExpandedBy(12f));
        PsycastsUIUtility.DrawAbility(inRect, ability);
        if (locked) Widgets.DrawRectFast(inRect, new Color(0f, 0f, 0f, 0.6f));

        TooltipHandler.TipRegion(
            inRect,
            () => $"{ability.LabelCap}\n\n{ability.description}{(unlockable ? "\n\n" + "VPE.ClickToUnlock".Translate().Resolve().ToUpper() : "")}",
            ability.GetHashCode());

        if (unlockable && Widgets.ButtonInvisible(inRect))
        {
            if (!this.devMode) this.hediff.SpentPoints();
            this.compAbilities.GiveAbility(ability);
        }
    }
}