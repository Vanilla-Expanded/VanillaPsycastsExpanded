using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using VFECore.UItils;
using AbilityDef = VFECore.Abilities.AbilityDef;

namespace VanillaPsycastsExpanded.UI;

[StaticConstructorOnStartup]
public class ITab_Pawn_Psycasts : ITab
{
    private readonly Dictionary<AbilityDef, Vector2> abilityPos = new();
    private readonly List<MeditationFocusDef> foci;
    private readonly Dictionary<string, List<PsycasterPathDef>> pathsByTab;
    private readonly List<TabRecord> tabs;
    private CompAbilities compAbilities;

    private string curTab;
    private bool devMode;

    private Hediff_PsycastAbilities hediff;
    private float lastPathsHeight;
    private int pathsPerRow;
    private Vector2 pathsScrollPos;
    private Pawn pawn;
    private Vector2 psysetsScrollPos;
    private bool smallMode;
    private bool useAltBackgrounds;

    static ITab_Pawn_Psycasts()
    {
        foreach (var def in DefDatabase<ThingDef>.AllDefs)
            if (def.race is { Humanlike: true })
            {
                def.inspectorTabs?.Add(typeof(ITab_Pawn_Psycasts));
                def.inspectorTabsResolved?.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Psycasts)));
            }
    }

    public ITab_Pawn_Psycasts()
    {
        labelKey = "VPE.Psycasts";
        size = new(Verse.UI.screenWidth, Verse.UI.screenHeight * 0.75f);
        pathsByTab = DefDatabase<PsycasterPathDef>.AllDefs.GroupBy(def => def.tab).ToDictionary(group => group.Key, group => group.ToList());
        foci = DefDatabase<MeditationFocusDef>.AllDefs.OrderByDescending(def => def.modContentPack.IsOfficialMod)
           .ThenByDescending(def => def.label)
           .ToList();
        tabs = pathsByTab.Select(kv => new TabRecord(kv.Key, () => curTab = kv.Key, () => curTab == kv.Key)).ToList();
        curTab = pathsByTab.Keys.FirstOrDefault();
    }

    public Vector2 Size => size;
    public float RequestedPsysetsHeight { get; private set; }

    public override bool IsVisible =>
        Find.Selector.SingleSelectedThing is Pawn pawn && pawn.health.hediffSet.HasHediff(VPE_DefOf.VPE_PsycastAbilityImplant) &&
        pawn.Faction is { IsPlayer: true };

    protected override void UpdateSize()
    {
        base.UpdateSize();
        size.y = PaneTopY - 30f;
        pathsPerRow = Mathf.FloorToInt(size.x * 0.67f / 200f);
        smallMode = PsycastsMod.Settings.smallMode switch
        {
            MultiCheckboxState.On => true,
            MultiCheckboxState.Off => false,
            _ => size.y <= 1080f / Prefs.UIScale
        };
    }

    public override void OnOpen()
    {
        base.OnOpen();
        pawn = (Pawn)Find.Selector.SingleSelectedThing;
        InitCache();
    }

    private void InitCache()
    {
        PsycastsUIUtility.Hediff = hediff = pawn.Psycasts();
        PsycastsUIUtility.CompAbilities = compAbilities = pawn.GetComp<CompAbilities>();
        abilityPos.Clear();
    }

    protected override void CloseTab()
    {
        base.CloseTab();
        pawn = null;
        PsycastsUIUtility.Hediff = hediff = null;
        PsycastsUIUtility.CompAbilities = compAbilities = null;
        abilityPos.Clear();
    }

    protected override void FillTab()
    {
        if (Find.Selector.SingleSelectedThing is Pawn p && pawn != p)
        {
            pawn = p;
            InitCache();
        }

        if (devMode && !Prefs.DevMode) devMode = false;

        if (pawn == null || hediff == null || compAbilities == null) return;
        var font = Text.Font;
        var anchor = Text.Anchor;
        Rect tabRect = new(Vector2.one * 20f, this.size - Vector2.one * 40f);
        var pawnAndStats = tabRect.TakeLeftPart(this.size.x * 0.3f);
        var paths = tabRect.ContractedBy(5f);
        Listing_Standard listing = new();
        listing.Begin(pawnAndStats);
        Text.Font = GameFont.Medium;
        listing.Label(pawn.Name.ToStringFull);
        listing.Label("VPE.PsyLevel".Translate(hediff.level));
        listing.Gap(10f);
        if (hediff.level < PsycastsMod.Settings.maxLevel)
        {
            var bar = listing.GetRect(60f).ContractedBy(10f, 0f);
            Text.Anchor = TextAnchor.MiddleCenter;
            var xpForNext = Hediff_PsycastAbilities.ExperienceRequiredForLevel(hediff.level + 1);
            if (devMode)
            {
                Text.Font = GameFont.Small;
                if (Widgets.ButtonText(bar.TakeRightPart(80f), "Dev: Level up"))
                    hediff.GainExperience(xpForNext, false);
                Text.Font = GameFont.Medium;
            }

            Widgets.FillableBar(bar, hediff.experience / xpForNext);
            Widgets.Label(bar, $"{hediff.experience.ToStringByStyle(ToStringStyle.FloatOne)} / {xpForNext}");
            Text.Font = GameFont.Tiny;
            listing.Label("VPE.EarnXP".Translate());
            listing.Gap(10f);
        }

        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        listing.Label("VPE.Points".Translate(hediff.points));
        Text.Font = GameFont.Tiny;
        listing.Label("VPE.SpendPoints".Translate());
        listing.Gap(3f);
        Text.Anchor = TextAnchor.MiddleLeft;
        Text.Font = GameFont.Small;
        var heightBefore = listing.CurHeight;
        if (listing.ButtonTextLabeled("VPE.PsycasterStats".Translate() + (smallMode ? $" ({"VPE.Hover".Translate()})" : ""), "VPE.Upgrade".Translate()))
        {
            var num = GenUI.CurrentAdjustmentMultiplier();
            if (devMode) hediff.ImproveStats(num);
            else if (hediff.points >= num)
            {
                hediff.SpentPoints(num);
                hediff.ImproveStats(num);
            }
            else Messages.Message("VPE.NotEnoughPoints".Translate(), MessageTypeDefOf.RejectInput, false);
        }

        var heightAfter = listing.CurHeight;
        if (smallMode)
        {
            Rect rect = new(pawnAndStats.x, heightBefore, pawnAndStats.width / 2f, heightAfter - heightBefore);
            if (Mouse.IsOver(rect))
            {
                Vector2 size = new(pawnAndStats.width, 150f);
                Find.WindowStack.ImmediateWindow(145 * 62346, new(GenUI.GetMouseAttachedWindowPos(size.x, size.y), size), WindowLayer.Super,
                    delegate
                    {
                        Listing_Standard inner = new();
                        inner.Begin(new(Vector2.one * 5f, size));
                        inner.StatDisplay(TexPsycasts.IconNeuralHeatLimit, StatDefOf.PsychicEntropyMax, pawn);
                        inner.StatDisplay(TexPsycasts.IconNeuralHeatRegenRate, StatDefOf.PsychicEntropyRecoveryRate, pawn);
                        inner.StatDisplay(TexPsycasts.IconPsychicSensitivity, StatDefOf.PsychicSensitivity, pawn);
                        if (PsycastsMod.Settings.changeFocusGain) inner.StatDisplay(TexPsycasts.IconPsyfocusGain, StatDefOf.MeditationFocusGain, pawn);
                        inner.StatDisplay(TexPsycasts.IconPsyfocusCost, VPE_DefOf.VPE_PsyfocusCostFactor, pawn);
                        inner.End();
                    });
            }
        }
        else
        {
            listing.StatDisplay(TexPsycasts.IconNeuralHeatLimit, StatDefOf.PsychicEntropyMax, pawn);
            listing.StatDisplay(TexPsycasts.IconNeuralHeatRegenRate, StatDefOf.PsychicEntropyRecoveryRate, pawn);
            listing.StatDisplay(TexPsycasts.IconPsychicSensitivity, StatDefOf.PsychicSensitivity, pawn);
            if (PsycastsMod.Settings.changeFocusGain) listing.StatDisplay(TexPsycasts.IconPsyfocusGain, StatDefOf.MeditationFocusGain, pawn);
            listing.StatDisplay(TexPsycasts.IconPsyfocusCost, VPE_DefOf.VPE_PsyfocusCostFactor, pawn);
        }

        listing.LabelWithIcon(TexPsycasts.IconFocusTypes, "VPE.FocusTypes".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        var fociRect = listing.GetRect(48f);
        var x = pawnAndStats.x;
        foreach (var def in foci)
        {
            if (x + 50f >= pawnAndStats.width)
            {
                x = pawnAndStats.x;
                listing.Gap(3f);
                fociRect = listing.GetRect(48f);
            }

            Rect rect = new(x, fociRect.y, 48f, 48f);
            DoFocus(rect, def);
            x += 50f;
        }

        listing.Gap(10f);
        if (smallMode)
        {
            if (listing.ButtonTextLabeled("VPE.PsysetCustomize".Translate(), "VPE.Edit".Translate())) Find.WindowStack.Add(new Dialog_EditPsysets(this));
        }
        else listing.Label("VPE.PsysetCustomize".Translate());

        Text.Font = GameFont.Tiny;
        listing.Label("VPE.PsysetDesc".Translate());
        Rect viewRect;
        if (!smallMode)
        {
            var psysetSectionHeight = pawnAndStats.height - listing.CurHeight;
            psysetSectionHeight -= 30;
            if (Prefs.DevMode)
                psysetSectionHeight -= 30;
            var psysets = listing.GetRect(psysetSectionHeight);
            Widgets.DrawMenuSection(psysets);
            viewRect = new(0, 0, psysets.width - 20f, RequestedPsysetsHeight);
            Widgets.BeginScrollView(psysets.ContractedBy(3f, 6f), ref psysetsScrollPos, viewRect);
            DoPsysets(viewRect);
            Widgets.EndScrollView();
        }

        listing.CheckboxLabeled("VPE.UseAltBackground".Translate(), ref useAltBackgrounds);
        if (Prefs.DevMode) listing.CheckboxLabeled("VPE.DevMode".Translate(), ref devMode);
        listing.End();
        if (pathsByTab.NullOrEmpty())
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Widgets.DrawMenuSection(paths);
            Widgets.Label(paths, "No Paths");
        }
        else
        {
            TabDrawer.DrawTabs(new(paths.x, paths.y + 40f, paths.width, paths.height), tabs);
            paths.yMin += 40f;
            Widgets.DrawMenuSection(paths);
            viewRect = new(0, 0, paths.width - 20f, lastPathsHeight);
            Widgets.BeginScrollView(paths.ContractedBy(2), ref pathsScrollPos, viewRect);
            DoPaths(viewRect);
            Widgets.EndScrollView();
        }

        Text.Font = font;
        Text.Anchor = anchor;
    }

    private void DoFocus(Rect inRect, MeditationFocusDef def)
    {
        Widgets.DrawBox(inRect, 3, Texture2D.grayTexture);
        var unlocked = def.CanPawnUse(pawn);
        var canUnlock = def.CanUnlock(pawn, out var lockedReason);
        GUI.color = unlocked ? Color.white : Color.gray;
        GUI.DrawTexture(inRect.ContractedBy(5f), def.Icon());
        GUI.color = Color.white;
        TooltipHandler.TipRegion(inRect, def.LabelCap + (def.description.NullOrEmpty() ? "" : "\n\n") +
                                         def.description + (canUnlock ? "" : $"\n\n{lockedReason}"));
        Widgets.DrawHighlightIfMouseover(inRect);
        if ((hediff.points >= 1 || devMode) && !unlocked && (canUnlock || devMode))
            if (Widgets.ButtonText(new(inRect.xMax - 13f, inRect.yMax - 13f, 12f, 12f), "▲"))
            {
                if (!devMode) hediff.SpentPoints();
                hediff.UnlockMeditationFocus(def);
            }
    }

    public void DoPsysets(Rect inRect)
    {
        Listing_Standard listing = new();
        listing.Begin(inRect);
        foreach (var psyset in hediff.psysets.ToList())
        {
            var rect = listing.GetRect(30f);
            Widgets.Label(rect.LeftHalf().LeftHalf(), psyset.Name);
            if (Widgets.ButtonText(rect.LeftHalf().RightHalf(), "VPE.Rename".Translate()))
                Find.WindowStack.Add(new Dialog_RenamePsyset(psyset));
            if (Widgets.ButtonText(rect.RightHalf().LeftHalf(), "VPE.Edit".Translate()))
                Find.WindowStack.Add(new Dialog_Psyset(psyset, pawn));
            if (Widgets.ButtonText(rect.RightHalf().RightHalf(), "VPE.Remove".Translate())) hediff.RemovePsySet(psyset);
        }

        if (Widgets.ButtonText(listing.GetRect(70f).LeftHalf().ContractedBy(5f), "VPE.CreatePsyset".Translate()))
        {
            PsySet psyset = new() { Name = "VPE.Untitled".Translate() };
            hediff.psysets.Add(psyset);
            Find.WindowStack.Add(new Dialog_Psyset(psyset, pawn));
        }

        RequestedPsysetsHeight = listing.CurHeight + 70f;
        listing.End();
    }

    private void DoPaths(Rect inRect)
    {
        var curPos = inRect.position + Vector2.one * 10f;
        var widthPerPath = (inRect.width - (pathsPerRow + 1) * 10f) / pathsPerRow;
        var maxHeight = 0f;
        var paths = pathsPerRow;
        foreach (var def in pathsByTab[curTab]
                    .OrderByDescending(hediff.unlockedPaths.Contains)
                    .ThenBy(path => path.order)
                    .ThenBy(path => path.label))
        {
            var texture = useAltBackgrounds ? def.backgroundImage : def.altBackgroundImage;
            var height = widthPerPath / texture.width * texture.height + 30f;
            Rect rect = new(curPos, new(widthPerPath, height));
            PsycastsUIUtility.DrawPathBackground(ref rect, def, useAltBackgrounds);
            if (hediff.unlockedPaths.Contains(def))
            {
                if (def.HasAbilities) PsycastsUIUtility.DoPathAbilities(rect, def, abilityPos, DoAbility);
            }
            else
            {
                Widgets.DrawRectFast(rect, new(0f, 0f, 0f, useAltBackgrounds ? 0.7f : 0.55f));
                if (hediff.points >= 1 || devMode)
                {
                    var centerRect = rect.CenterRect(new(140f, 30f));
                    if (devMode || def.CanPawnUnlock(pawn))
                    {
                        if (Widgets.ButtonText(centerRect, "VPE.Unlock".Translate()))
                        {
                            if (!devMode) hediff.SpentPoints();
                            hediff.UnlockPath(def);
                        }
                    }
                    else
                    {
                        GUI.color = Color.grey;
                        var label = "VPE.Locked".Translate().Resolve() + ": " + def.lockedReason;
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

            maxHeight = Mathf.Max(maxHeight, height + 10f);
            curPos.x += widthPerPath + 10f;
            paths--;
            if (paths == 0)
            {
                curPos.x = inRect.x + 10f;
                curPos.y += maxHeight;
                paths = pathsPerRow;
                maxHeight = 0f;
            }
        }

        lastPathsHeight = curPos.y + maxHeight;
    }

    private void DoAbility(Rect inRect, AbilityDef ability)
    {
        var unlockable = false;
        var locked = false;
        if (!compAbilities.HasAbility(ability))
        {
            if (devMode || (ability.Psycast().PrereqsCompleted(compAbilities) && hediff.points >= 1))
                unlockable = true;
            else locked = true;
        }

        if (unlockable) Widgets.DrawStrongHighlight(inRect.ExpandedBy(12f));
        PsycastsUIUtility.DrawAbility(inRect, ability);
        if (locked) Widgets.DrawRectFast(inRect, new(0f, 0f, 0f, 0.6f));

        TooltipHandler.TipRegion(
            inRect,
            () => $"{ability.LabelCap}\n\n{ability.description}{(unlockable ? "\n\n" + "VPE.ClickToUnlock".Translate().Resolve().ToUpper() : "")}",
            ability.GetHashCode());

        if (unlockable && Widgets.ButtonInvisible(inRect))
        {
            if (!devMode) hediff.SpentPoints();
            compAbilities.GiveAbility(ability);
        }
    }
}
