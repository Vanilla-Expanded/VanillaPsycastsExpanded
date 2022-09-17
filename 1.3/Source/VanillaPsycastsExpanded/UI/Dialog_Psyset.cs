namespace VanillaPsycastsExpanded.UI
{
    using System.Collections.Generic;
    using System.Linq;
    using Technomancer;
    using UnityEngine;
    using Verse;
    using VFECore.Abilities;
    using VFECore.UItils;

    public class Dialog_Psyset : Window
    {
        private readonly PsySet                          psyset;
        private readonly Hediff_PsycastAbilities         hediff;
        private readonly CompAbilities                   compAbilities;
        private readonly Dictionary<AbilityDef, Vector2> abilityPos = new();
        private          Pawn                            pawn;
        private          int                             curIdx;
        public           List<PsycasterPathDef>          paths;

        public Dialog_Psyset(PsySet psyset, Pawn pawn)
        {
            this.psyset                = psyset;
            this.pawn                  = pawn;
            this.hediff                = pawn.Psycasts();
            this.compAbilities         = pawn.GetComp<CompAbilities>();
            this.doCloseButton         = true;
            this.doCloseX              = true;
            this.forcePause            = true;
            this.closeOnClickedOutside = true;
            this.paths                 = this.hediff.unlockedPaths.ListFullCopy();
            foreach (PsycasterPathDef path in pawn.AllPathsFromPsyrings())
                if (!this.paths.Contains(path))
                    this.paths.Add(path);
        }

        public override Vector2 InitialSize => new(480f, 520f);

        public override void DoWindowContents(Rect inRect)
        {
            inRect.yMax -= 50f;
            Text.Font   =  GameFont.Medium;
            Widgets.Label(inRect.TakeTopPart(40f).LeftHalf(), this.psyset.Name);
            Text.Font = GameFont.Small;
            int  group        = DragAndDropWidget.NewGroup();
            Rect existingRect = inRect.LeftHalf().ContractedBy(3f);
            existingRect.xMax -= 8f;
            Widgets.Label(existingRect.TakeTopPart(20f), "VPE.Contents".Translate());
            Widgets.DrawMenuSection(existingRect);
            DragAndDropWidget.DropArea(group, existingRect, obj => this.psyset.Abilities.Add((AbilityDef) obj), null);
            Vector2 curPos = existingRect.position + new Vector2(8f, 8f);
            foreach (AbilityDef def in this.psyset.Abilities.ToList())
            {
                Rect rect = new(curPos, new Vector2(36f, 36f));
                PsycastsUIUtility.DrawAbility(rect, def);
                TooltipHandler.TipRegion(rect, () => $"{def.LabelCap}\n\n{def.description}\n\n{"VPE.ClickRemove".Translate().Resolve().ToUpper()}",
                                         def.GetHashCode() + 2);
                if (Widgets.ButtonInvisible(rect)) this.psyset.Abilities.Remove(def);
                curPos.x += 44f;
                if (curPos.x + 36f >= existingRect.xMax)
                {
                    curPos.x =  existingRect.xMin + 8f;
                    curPos.y += 44f;
                }
            }

            Rect abilityRect  = inRect.RightHalf().ContractedBy(3f);
            Rect pagesRect    = abilityRect.TakeTopPart(50f);
            Rect decreaseRect = pagesRect.TakeLeftPart(40f).ContractedBy(0f, 5f);
            Rect increaseRect = pagesRect.TakeRightPart(40f).ContractedBy(0f, 5f);
            if (this.curIdx > 0                    && Widgets.ButtonText(decreaseRect, "<")) this.curIdx--;
            if (this.curIdx < this.paths.Count - 1 && Widgets.ButtonText(increaseRect, ">")) this.curIdx++;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(pagesRect, $"{(this.paths.Count > 0 ? this.curIdx + 1 : 0)} / {this.paths.Count}");
            Text.Anchor = TextAnchor.UpperLeft;
            if (this.paths.Count > 0)
            {
                PsycasterPathDef path = this.paths[this.curIdx];
                PsycastsUIUtility.DrawPathBackground(ref abilityRect, path);
                PsycastsUIUtility.DoPathAbilities(abilityRect, path, this.abilityPos, (rect, def) =>
                {
                    PsycastsUIUtility.DrawAbility(rect, def);
                    if (this.compAbilities.HasAbility(def))
                    {
                        DragAndDropWidget.Draggable(group, rect, def);
                        TooltipHandler.TipRegion(rect, () => $"{def.LabelCap}\n\n{def.description}", def.GetHashCode() + 1);
                    }
                    else
                        Widgets.DrawRectFast(rect, new Color(0f, 0f, 0f, 0.6f));
                });
            }

            if (DragAndDropWidget.CurrentlyDraggedDraggable() is AbilityDef abilityDef)
                PsycastsUIUtility.DrawAbility(new Rect(Event.current.mousePosition, new Vector2(36f, 36f)), abilityDef);
            if (DragAndDropWidget.HoveringDropAreaRect(group) is { } hovering) Widgets.DrawHighlight(hovering);
        }
    }

    public class Dialog_RenamePsyset : Dialog_Rename
    {
        private readonly PsySet psyset;

        public Dialog_RenamePsyset(PsySet psyset)
        {
            this.psyset  = psyset;
            this.curName = psyset.Name;
        }

        protected override void SetName(string name)
        {
            this.psyset.Name = name;
        }
    }
}