using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Command_Ability = VFECore.Abilities.Command_Ability;

namespace VanillaPsycastsExpanded.UI;

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

    private readonly Texture2D LimitedTex;

    private readonly Pawn_PsychicEntropyTracker tracker;

    private readonly Texture2D UnlimitedTex;

    private bool draggingPsyfocusBar;

    private float selectedPsyfocusTarget = -1f;

    public PsychicStatusGizmo(Pawn_PsychicEntropyTracker tracker)
    {
        this.tracker = tracker;
        Order = -100f;
        LimitedTex = ContentFinder<Texture2D>.Get("UI/Icons/EntropyLimit/Limited");
        UnlimitedTex = ContentFinder<Texture2D>.Get("UI/Icons/EntropyLimit/Unlimited");
    }

    private static void DrawThreshold(Rect rect, float percent, float entropyValue)
    {
        Rect position = new()
        {
            x = rect.x + 3f + (rect.width - 8f) * percent,
            y = rect.y + rect.height - 9f,
            width = 2f,
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
        var num = Mathf.Round((rect.width - 8f) * percent);
        GUI.DrawTexture(new Rect
        {
            x = rect.x + 3f + num,
            y = rect.y,
            width = 2f,
            height = rect.height
        }, PsyfocusTargetTex);
        var num2 = Widgets.AdjustCoordToUIScalingFloor(rect.x + 2f + num);
        var xMax = Widgets.AdjustCoordToUIScalingCeil(num2 + 4f);
        Rect rect2 = new()
        {
            y = rect.y - 3f,
            height = 5f,
            xMin = num2,
            xMax = xMax
        };
        GUI.DrawTexture(rect2, PsyfocusTargetTex);
        var position = rect2;
        position.y = rect.yMax - 2f;
        GUI.DrawTexture(position, PsyfocusTargetTex);
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        Rect rect = new(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
        var rect2 = rect.ContractedBy(6f);
        var num = Mathf.Repeat(Time.time, 0.85f);

        var psycast = (((MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow).LastMouseoverGizmo as Command_Ability)?.ability?.def
            ?.GetModExtension<AbilityExtension_Psycast>();

        var num2 = num switch
        {
            < 0.1f => num / 0.1f,
            >= 0.25f => 1f - (num - 0.25f) / 0.6f,
            _ => 1f
        };
        Widgets.DrawWindowBackground(rect);
        Text.Font = GameFont.Small;
        var rect3 = rect2;
        rect3.y += 6f;
        rect3.height = Text.LineHeight;
        Widgets.Label(rect3, "PsychicEntropyShort".Translate());
        var rect4 = rect2;
        rect4.y += 38f;
        rect4.height = Text.LineHeight;
        Widgets.Label(rect4, "PsyfocusLabelGizmo".Translate());
        var rect5 = rect2;
        rect5.x += 63f;
        rect5.y += 6f;
        rect5.width = 100f;
        rect5.height = 22f;
        var entropyRelativeValue = tracker.EntropyRelativeValue;
        Widgets.FillableBar(rect5, Mathf.Min(entropyRelativeValue, 1f), EntropyBarTex, EmptyBarTex, true);
        if (tracker.EntropyValue > tracker.MaxEntropy)
            Widgets.FillableBar(rect5, Mathf.Min(entropyRelativeValue - 1f, 1f), OverLimitBarTex, EntropyBarTex, true);
        if (psycast != null)
        {
            var entropyGain = psycast.GetEntropyUsedByPawn(tracker.Pawn);
            if (entropyGain > 1.401298E-45f)
            {
                var rect6 = rect5.ContractedBy(3f);
                var width = rect6.width;
                var num3 = tracker.EntropyToRelativeValue(tracker.EntropyValue + entropyGain);
                var num4 = entropyRelativeValue;
                if (num4 > 1f)
                {
                    num4 -= 1f;
                    num3 -= 1f;
                }

                rect6.xMin = Widgets.AdjustCoordToUIScalingFloor(rect6.xMin + num4 * width);
                rect6.width = Widgets.AdjustCoordToUIScalingFloor(Mathf.Max(Mathf.Min(num3, 1f) - num4, 0f) * width);

                GUI.color = new Color(1f, 1f, 1f, num2 * 0.7f);
                GenUI.DrawTextureWithMaterial(rect6, EntropyBarTexAdd, null);
                GUI.color = Color.white;
            }
        }

        if (tracker.EntropyValue > tracker.MaxEntropy)
            foreach (var keyValuePair in Pawn_PsychicEntropyTracker.EntropyThresholds)
                if (keyValuePair.Value > 1f && keyValuePair.Value < 2f)
                    DrawThreshold(rect5, keyValuePair.Value - 1f, entropyRelativeValue);

        var label = tracker.EntropyValue.ToString("F0") + " / " + tracker.MaxEntropy.ToString("F0");
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(rect5, label);
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Tiny;
        GUI.color = Color.white;
        var rect7 = rect2;
        rect7.width = 175f;
        rect7.height = 38f;
        TooltipHandler.TipRegion(rect7, delegate
        {
            var f = tracker.EntropyValue / tracker.RecoveryRate;
            return string.Format("PawnTooltipPsychicEntropyStats".Translate(), Mathf.Round(tracker.EntropyValue), Mathf.Round(tracker.MaxEntropy),
                tracker.RecoveryRate.ToString("0.#"), Mathf.Round(f)) + "\n\n" + "PawnTooltipPsychicEntropyDesc".Translate();
        }, Gen.HashCombineInt(tracker.GetHashCode(), 133858));
        var rect8 = rect2;
        rect8.x += 63f;
        rect8.y += 38f;
        rect8.width = 100f;
        rect8.height = 22f;
        var flag = Mouse.IsOver(rect8);
        Widgets.FillableBar(rect8, Mathf.Min(tracker.CurrentPsyfocus, 1f), flag ? PsyfocusBarHighlightTex : PsyfocusBarTex, EmptyBarTex, true);
        if (psycast != null)
        {
            var usedPsyfocus = psycast.GetPsyfocusUsedByPawn(tracker.Pawn);
            if (usedPsyfocus > 1.401298E-45f)
            {
                var rect9 = rect8.ContractedBy(3f);
                var num5 = Mathf.Max(tracker.CurrentPsyfocus - usedPsyfocus, 0f);
                var width2 = rect9.width;
                rect9.xMin = Widgets.AdjustCoordToUIScalingFloor(rect9.xMin + num5 * width2);
                rect9.width = Widgets.AdjustCoordToUIScalingCeil((tracker.CurrentPsyfocus - num5) * width2);
                GUI.color = new Color(1f, 1f, 1f, num2);
                GenUI.DrawTextureWithMaterial(rect9, PsyfocusBarTexReduce, null);
                GUI.color = Color.white;
            }
        }

        for (var i = 1; i < Pawn_PsychicEntropyTracker.PsyfocusBandPercentages.Count - 1; i++)
            DrawThreshold(rect8, Pawn_PsychicEntropyTracker.PsyfocusBandPercentages[i], tracker.CurrentPsyfocus);
        var num6 = Mathf.Clamp(Mathf.Round((Event.current.mousePosition.x - (rect8.x + 3f)) / (rect8.width - 8f) * 16f) / 16f, 0f, 1f);
        var current = Event.current;
        if (current.type == EventType.MouseDown && current.button == 0 && flag)
        {
            selectedPsyfocusTarget = num6;
            draggingPsyfocusBar = true;
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.MeditationDesiredPsyfocus, KnowledgeAmount.Total);
            SoundDefOf.DragSlider.PlayOneShotOnCamera();
            current.Use();
        }

        if (current.type == EventType.MouseDrag && current.button == 0 && draggingPsyfocusBar && flag)
        {
            if (Math.Abs(num6 - selectedPsyfocusTarget) > 1.401298E-45f) SoundDefOf.DragSlider.PlayOneShotOnCamera();
            selectedPsyfocusTarget = num6;
            current.Use();
        }

        if (current.type == EventType.MouseUp && current.button == 0 && draggingPsyfocusBar)
        {
            if (selectedPsyfocusTarget >= 0f) tracker.SetPsyfocusTarget(selectedPsyfocusTarget);
            selectedPsyfocusTarget = -1f;
            draggingPsyfocusBar = false;
            current.Use();
        }

        UIHighlighter.HighlightOpportunity(rect8, "PsyfocusBar");
        DrawPsyfocusTarget(rect8, draggingPsyfocusBar ? selectedPsyfocusTarget : tracker.TargetPsyfocus);
        GUI.color = Color.white;
        var rect10 = rect2;
        rect10.y += 38f;
        rect10.width = 175f;
        rect10.height = 38f;
        TooltipHandler.TipRegion(rect10, () => tracker.PsyfocusTipString(selectedPsyfocusTarget),
            Gen.HashCombineInt(tracker.GetHashCode(), 133873));
        if (tracker.Pawn.IsColonistPlayerControlled)
        {
            Rect limitButton = new(rect2.x + (rect2.width - 32f), rect2.y + (rect2.height / 2f - 32f + 4f), 32f, 32f);
            if (Widgets.ButtonImage(limitButton, tracker.limitEntropyAmount ? LimitedTex : UnlimitedTex))
            {
                tracker.limitEntropyAmount = !tracker.limitEntropyAmount;
                if (tracker.limitEntropyAmount)
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                else
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }

            TooltipHandler.TipRegionByKey(limitButton, "PawnTooltipPsychicEntropyLimit");
        }

        if (TryGetPainMultiplier(tracker.Pawn, out var painMultiplier))
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            var recoveryBonus = (painMultiplier - 1f).ToStringPercent("F0");
            var widthCached = recoveryBonus.GetWidthCached();
            var rect12 = rect2;
            rect12.x += rect2.width - widthCached / 2f - 16f;
            rect12.y += 38f;
            rect12.width = widthCached;
            rect12.height = Text.LineHeight;
            GUI.color = PainBoostColor;
            Widgets.Label(rect12, recoveryBonus);
            GUI.color = Color.white;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect12.ContractedBy(-1f),
                () => "PawnTooltipPsychicEntropyPainFocus".Translate(
                    tracker.Pawn.health.hediffSet.PainTotal.ToStringPercent("F0"), recoveryBonus),
                Gen.HashCombineInt(tracker.GetHashCode(), 133878));
        }

        return new GizmoResult(GizmoState.Clear);
    }

    private static bool TryGetPainMultiplier(Pawn pawn, out float painMultiplier)
    {
        var parts = StatDefOf.PsychicEntropyRecoveryRate.parts;
        for (var i = 0; i < parts.Count; i++)
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