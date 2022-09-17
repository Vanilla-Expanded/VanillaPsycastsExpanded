namespace VanillaPsycastsExpanded.UI
{
    using System;
    using System.Collections.Generic;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using Command_Ability = VFECore.Abilities.Command_Ability;

    [StaticConstructorOnStartup]
    public class PsychicStatusGizmo : Gizmo
    {
        private static readonly Color PainBoostColor = new(0.2f, 0.65f, 0.35f);

        private static readonly Texture2D EntropyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.46f, 0.34f, 0.35f));

        private static readonly Texture2D EntropyBarTexAdd = SolidColorMaterials.NewSolidColorTexture(new Color(0.78f, 0.72f, 0.66f));

        private static readonly Texture2D OverLimitBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.75f, 0.2f, 0.15f));

        private static readonly Texture2D PsyfocusBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.43f));

        private static readonly Texture2D PsyfocusBarTexReduce = SolidColorMaterials.NewSolidColorTexture(new Color(0.65f, 0.83f, 0.83f));

        private static readonly Texture2D PsyfocusBarHighlightTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.43f, 0.54f, 0.55f));

        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

        private static readonly Texture2D PsyfocusTargetTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.74f, 0.97f, 0.8f));

        private readonly Pawn_PsychicEntropyTracker tracker;

        private readonly Texture2D LimitedTex;

        private readonly Texture2D UnlimitedTex;

        private float selectedPsyfocusTarget = -1f;

        private bool draggingPsyfocusBar;

        public PsychicStatusGizmo(Pawn_PsychicEntropyTracker tracker)
        {
            this.tracker      = tracker;
            this.order        = -100f;
            this.LimitedTex   = ContentFinder<Texture2D>.Get("UI/Icons/EntropyLimit/Limited");
            this.UnlimitedTex = ContentFinder<Texture2D>.Get("UI/Icons/EntropyLimit/Unlimited");
        }

        private static void DrawThreshold(Rect rect, float percent, float entropyValue)
        {
            Rect position = new()
            {
                x      = rect.x               + 3f + (rect.width - 8f) * percent,
                y      = rect.y + rect.height - 9f,
                width  = 2f,
                height = 6f
            };
            if (entropyValue < percent)
            {
                GUI.DrawTexture(position, BaseContent.GreyTex);
                return;
            }

            GUI.DrawTexture(position, BaseContent.BlackTex);
        }

        private static void DrawPsyfocusTarget(Rect rect, float percent)
        {
            float num = Mathf.Round((rect.width - 8f) * percent);
            GUI.DrawTexture(new Rect
            {
                x      = rect.x + 3f + num,
                y      = rect.y,
                width  = 2f,
                height = rect.height
            }, PsyfocusTargetTex);
            float num2 = Widgets.AdjustCoordToUIScalingFloor(rect.x + 2f + num);
            float xMax = Widgets.AdjustCoordToUIScalingCeil(num2    + 4f);
            Rect rect2 = new()
            {
                y      = rect.y - 3f,
                height = 5f,
                xMin   = num2,
                xMax   = xMax
            };
            GUI.DrawTexture(rect2, PsyfocusTargetTex);
            Rect position = rect2;
            position.y = rect.yMax - 2f;
            GUI.DrawTexture(position, PsyfocusTargetTex);
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect  rect  = new(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            Rect  rect2 = rect.ContractedBy(6f);
            float num   = Mathf.Repeat(Time.time, 0.85f);

            AbilityExtension_Psycast psycast = (((MainTabWindow_Inspect) MainButtonDefOf.Inspect.TabWindow).LastMouseoverGizmo as Command_Ability)?.ability?.def
                ?.GetModExtension<AbilityExtension_Psycast>();

            float num2 = num switch
            {
                < 0.1f   => num / 0.1f,
                >= 0.25f => 1f - (num - 0.25f) / 0.6f,
                _        => 1f
            };
            Widgets.DrawWindowBackground(rect);
            Text.Font = GameFont.Small;
            Rect rect3 = rect2;
            rect3.y      += 6f;
            rect3.height =  Text.LineHeight;
            Widgets.Label(rect3, "PsychicEntropyShort".Translate());
            Rect rect4 = rect2;
            rect4.y      += 38f;
            rect4.height =  Text.LineHeight;
            Widgets.Label(rect4, "PsyfocusLabelGizmo".Translate());
            Rect rect5 = rect2;
            rect5.x      += 63f;
            rect5.y      += 6f;
            rect5.width  =  100f;
            rect5.height =  22f;
            float entropyRelativeValue = this.tracker.EntropyRelativeValue;
            Widgets.FillableBar(rect5, Mathf.Min(entropyRelativeValue, 1f), EntropyBarTex, EmptyBarTex, true);
            if (this.tracker.EntropyValue > this.tracker.MaxEntropy)
                Widgets.FillableBar(rect5, Mathf.Min(entropyRelativeValue - 1f, 1f), OverLimitBarTex, EntropyBarTex, true);
            if (psycast != null)
            {
                float entropyGain = psycast.GetEntropyUsedByPawn(this.tracker.Pawn);
                if (entropyGain > 1.401298E-45f)
                {
                    Rect  rect6 = rect5.ContractedBy(3f);
                    float width = rect6.width;
                    float num3  = this.tracker.EntropyToRelativeValue(this.tracker.EntropyValue + entropyGain);
                    float num4  = entropyRelativeValue;
                    if (num4 > 1f)
                    {
                        num4 -= 1f;
                        num3 -= 1f;
                    }

                    rect6.xMin  = Widgets.AdjustCoordToUIScalingFloor(rect6.xMin + num4 * width);
                    rect6.width = Widgets.AdjustCoordToUIScalingFloor(Mathf.Max(Mathf.Min(num3, 1f) - num4, 0f) * width);

                    GUI.color = new Color(1f, 1f, 1f, num2 * 0.7f);
                    GenUI.DrawTextureWithMaterial(rect6, EntropyBarTexAdd, null);
                    GUI.color = Color.white;
                }
            }

            if (this.tracker.EntropyValue > this.tracker.MaxEntropy)
                foreach (KeyValuePair<PsychicEntropySeverity, float> keyValuePair in Pawn_PsychicEntropyTracker.EntropyThresholds)
                    if (keyValuePair.Value > 1f && keyValuePair.Value < 2f)
                        DrawThreshold(rect5, keyValuePair.Value - 1f, entropyRelativeValue);

            string label = this.tracker.EntropyValue.ToString("F0") + " / " + this.tracker.MaxEntropy.ToString("F0");
            Text.Font   = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect5, label);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font   = GameFont.Tiny;
            GUI.color   = Color.white;
            Rect rect7 = rect2;
            rect7.width  = 175f;
            rect7.height = 38f;
            TooltipHandler.TipRegion(rect7, delegate
            {
                float f = this.tracker.EntropyValue / this.tracker.RecoveryRate;
                return string.Format("PawnTooltipPsychicEntropyStats".Translate(), Mathf.Round(this.tracker.EntropyValue), Mathf.Round(this.tracker.MaxEntropy),
                                     this.tracker.RecoveryRate.ToString("0.#"), Mathf.Round(f)) + "\n\n" + "PawnTooltipPsychicEntropyDesc".Translate();
            }, Gen.HashCombineInt(this.tracker.GetHashCode(), 133858));
            Rect rect8 = rect2;
            rect8.x      += 63f;
            rect8.y      += 38f;
            rect8.width  =  100f;
            rect8.height =  22f;
            bool flag = Mouse.IsOver(rect8);
            Widgets.FillableBar(rect8, Mathf.Min(this.tracker.CurrentPsyfocus, 1f), flag ? PsyfocusBarHighlightTex : PsyfocusBarTex, EmptyBarTex, true);
            if (psycast != null)
            {
                float usedPsyfocus = psycast.GetPsyfocusUsedByPawn(this.tracker.Pawn);
                if (usedPsyfocus > 1.401298E-45f)
                {
                    Rect  rect9  = rect8.ContractedBy(3f);
                    float num5   = Mathf.Max(this.tracker.CurrentPsyfocus - usedPsyfocus, 0f);
                    float width2 = rect9.width;
                    rect9.xMin  = Widgets.AdjustCoordToUIScalingFloor(rect9.xMin + num5 * width2);
                    rect9.width = Widgets.AdjustCoordToUIScalingCeil((this.tracker.CurrentPsyfocus - num5) * width2);
                    GUI.color   = new Color(1f, 1f, 1f, num2);
                    GenUI.DrawTextureWithMaterial(rect9, PsyfocusBarTexReduce, null);
                    GUI.color = Color.white;
                }
            }

            for (int i = 1; i < Pawn_PsychicEntropyTracker.PsyfocusBandPercentages.Count - 1; i++)
                DrawThreshold(rect8, Pawn_PsychicEntropyTracker.PsyfocusBandPercentages[i], this.tracker.CurrentPsyfocus);
            float num6    = Mathf.Clamp(Mathf.Round((Event.current.mousePosition.x - (rect8.x + 3f)) / (rect8.width - 8f) * 16f) / 16f, 0f, 1f);
            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0 && flag)
            {
                this.selectedPsyfocusTarget = num6;
                this.draggingPsyfocusBar    = true;
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.MeditationDesiredPsyfocus, KnowledgeAmount.Total);
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
                current.Use();
            }

            if (current.type == EventType.MouseDrag && current.button == 0 && this.draggingPsyfocusBar && flag)
            {
                if (Math.Abs(num6 - this.selectedPsyfocusTarget) > 1.401298E-45f) SoundDefOf.DragSlider.PlayOneShotOnCamera();
                this.selectedPsyfocusTarget = num6;
                current.Use();
            }

            if (current.type == EventType.MouseUp && current.button == 0 && this.draggingPsyfocusBar)
            {
                if (this.selectedPsyfocusTarget >= 0f) this.tracker.SetPsyfocusTarget(this.selectedPsyfocusTarget);
                this.selectedPsyfocusTarget = -1f;
                this.draggingPsyfocusBar    = false;
                current.Use();
            }

            UIHighlighter.HighlightOpportunity(rect8, "PsyfocusBar");
            DrawPsyfocusTarget(rect8, this.draggingPsyfocusBar ? this.selectedPsyfocusTarget : this.tracker.TargetPsyfocus);
            GUI.color = Color.white;
            Rect rect10 = rect2;
            rect10.y      += 38f;
            rect10.width  =  175f;
            rect10.height =  38f;
            TooltipHandler.TipRegion(rect10, () => this.tracker.PsyfocusTipString(this.selectedPsyfocusTarget),
                                     Gen.HashCombineInt(this.tracker.GetHashCode(), 133873));
            if (this.tracker.Pawn.IsColonistPlayerControlled)
            {
                Rect limitButton = new(rect2.x + (rect2.width - 32f), rect2.y + (rect2.height / 2f - 32f + 4f), 32f, 32f);
                if (Widgets.ButtonImage(limitButton, this.tracker.limitEntropyAmount ? this.LimitedTex : this.UnlimitedTex))
                {
                    this.tracker.limitEntropyAmount = !this.tracker.limitEntropyAmount;
                    if (this.tracker.limitEntropyAmount)
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    else
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }

                TooltipHandler.TipRegionByKey(limitButton, "PawnTooltipPsychicEntropyLimit");
            }

            if (TryGetPainMultiplier(this.tracker.Pawn, out float painMultiplier))
            {
                Text.Font   = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                string recoveryBonus = (painMultiplier - 1f).ToStringPercent("F0");
                float  widthCached   = recoveryBonus.GetWidthCached();
                Rect   rect12        = rect2;
                rect12.x      += rect2.width - widthCached / 2f - 16f;
                rect12.y      += 38f;
                rect12.width  =  widthCached;
                rect12.height =  Text.LineHeight;
                GUI.color     =  PainBoostColor;
                Widgets.Label(rect12, recoveryBonus);
                GUI.color   = Color.white;
                Text.Font   = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperLeft;
                TooltipHandler.TipRegion(rect12.ContractedBy(-1f),
                                         () => "PawnTooltipPsychicEntropyPainFocus".Translate(
                                             this.tracker.Pawn.health.hediffSet.PainTotal.ToStringPercent("F0"), recoveryBonus),
                                         Gen.HashCombineInt(this.tracker.GetHashCode(), 133878));
            }

            return new GizmoResult(GizmoState.Clear);
        }

        private static bool TryGetPainMultiplier(Pawn pawn, out float painMultiplier)
        {
            List<StatPart> parts = StatDefOf.PsychicEntropyRecoveryRate.parts;
            for (int i = 0; i < parts.Count; i++)
                if (parts[i] is StatPart_Pain statPart_Pain)
                {
                    painMultiplier = statPart_Pain.PainFactor(pawn);
                    return true;
                }

            painMultiplier = 0f;
            return false;
        }

        public override float GetWidth(float maxWidth) => 212f;
    }
}