using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VanillaPsycastsExpanded.Technomancer;
using Verse;
using VEF.Abilities;
using VEF.Utils;

namespace VanillaPsycastsExpanded.UI;

public class Dialog_Psyset : Window
{
    private readonly Dictionary<AbilityDef, Vector2> abilityPos = new();
    private readonly CompAbilities compAbilities;
    private readonly Hediff_PsycastAbilities hediff;
    private readonly PsySet psyset;
    public List<PsycasterPathDef> paths;
    private int curIdx;
    private Pawn pawn;

    public Dialog_Psyset(PsySet psyset, Pawn pawn)
    {
        this.psyset = psyset;
        this.pawn = pawn;
        hediff = pawn.Psycasts();
        compAbilities = pawn.GetComp<CompAbilities>();
        doCloseButton = true;
        doCloseX = true;
        forcePause = true;
        closeOnClickedOutside = true;
        paths = hediff.unlockedPaths.ListFullCopy();
        foreach (var path in pawn.AllPathsFromPsyrings())
            if (!paths.Contains(path))
                paths.Add(path);
    }

    public override Vector2 InitialSize => new(480f, 520f);

    public override void DoWindowContents(Rect inRect)
    {
        inRect.yMax -= 50f;
        Text.Font = GameFont.Medium;
        Widgets.Label(inRect.TakeTopPart(40f).LeftHalf(), psyset.Name);
        Text.Font = GameFont.Small;
        var group = DragAndDropWidget.NewGroup();
        var existingRect = inRect.LeftHalf().ContractedBy(3f);
        existingRect.xMax -= 8f;
        Widgets.Label(existingRect.TakeTopPart(20f), "VPE.Contents".Translate());
        Widgets.DrawMenuSection(existingRect);
        DragAndDropWidget.DropArea(group, existingRect, obj => psyset.Abilities.Add((AbilityDef)obj), null);
        var curPos = existingRect.position + new Vector2(8f, 8f);
        foreach (var def in psyset.Abilities.ToList())
        {
            Rect rect = new(curPos, new(36f, 36f));
            PsycastsUIUtility.DrawAbility(rect, def);
            TooltipHandler.TipRegion(rect, () => $"{def.LabelCap}\n\n{def.description}\n\n{"VPE.ClickRemove".Translate().Resolve().ToUpper()}",
                def.GetHashCode() + 2);
            if (Widgets.ButtonInvisible(rect)) psyset.Abilities.Remove(def);
            curPos.x += 44f;
            if (curPos.x + 36f >= existingRect.xMax)
            {
                curPos.x = existingRect.xMin + 8f;
                curPos.y += 44f;
            }
        }

        var abilityRect = inRect.RightHalf().ContractedBy(3f);
        var pagesRect = abilityRect.TakeTopPart(50f);
        var decreaseRect = pagesRect.TakeLeftPart(40f).ContractedBy(0f, 5f);
        var increaseRect = pagesRect.TakeRightPart(40f).ContractedBy(0f, 5f);
        if (curIdx > 0 && Widgets.ButtonText(decreaseRect, "<")) curIdx--;
        if (curIdx < paths.Count - 1 && Widgets.ButtonText(increaseRect, ">")) curIdx++;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(pagesRect, $"{(paths.Count > 0 ? curIdx + 1 : 0)} / {paths.Count}");
        Text.Anchor = TextAnchor.UpperLeft;
        if (paths.Count > 0)
        {
            var path = paths[curIdx];
            PsycastsUIUtility.DrawPathBackground(ref abilityRect, path);
            PsycastsUIUtility.DoPathAbilities(abilityRect, path, abilityPos, (rect, def) =>
            {
                PsycastsUIUtility.DrawAbility(rect, def);
                if (compAbilities.HasAbility(def))
                {
                    DragAndDropWidget.Draggable(group, rect, def);
                    TooltipHandler.TipRegion(rect, () => $"{def.LabelCap}\n\n{def.description}", def.GetHashCode() + 1);
                }
                else
                    Widgets.DrawRectFast(rect, new(0f, 0f, 0f, 0.6f));
            });
        }

        if (DragAndDropWidget.CurrentlyDraggedDraggable() is AbilityDef abilityDef)
            PsycastsUIUtility.DrawAbility(new(Event.current.mousePosition, new(36f, 36f)), abilityDef);
        if (DragAndDropWidget.HoveringDropAreaRect(group) is { } hovering) Widgets.DrawHighlight(hovering);
    }
}

public class Dialog_RenamePsyset : Dialog_Rename<PsySet>
{
    public Dialog_RenamePsyset(PsySet psyset) : base(psyset) { }
}
