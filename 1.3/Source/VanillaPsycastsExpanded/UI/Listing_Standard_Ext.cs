namespace VanillaPsycastsExpanded;

using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

public static class Listing_Standard_Ext
{
    public static void CheckboxMultiLabeled(this Listing_Standard listing, string label, ref MultiCheckboxState state, string tooltip = null)
    {
        Rect rect = listing.GetRect(Text.LineHeight);
        if (listing.BoundingRectCached == null || rect.Overlaps(listing.BoundingRectCached.Value))
        {
            if (!tooltip.NullOrEmpty())
            {
                MouseoverSounds.DoRegion(rect);

                if (Mouse.IsOver(rect)) Widgets.DrawHighlight(rect);

                TooltipHandler.TipRegion(rect, tooltip);
            }

            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(rect, label);
            if (Widgets.ButtonInvisible(rect))
            {
                state = state switch
                {
                    MultiCheckboxState.On      => MultiCheckboxState.Partial,
                    MultiCheckboxState.Partial => MultiCheckboxState.Off,
                    MultiCheckboxState.Off     => MultiCheckboxState.On,
                    _                          => throw new ArgumentOutOfRangeException(nameof(state), state, null)
                };

                if (state == MultiCheckboxState.On)
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                else
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
            }

            GUI.DrawTexture(new Rect(rect.x + rect.width - 24f, rect.y, 24f, 24f), state switch
            {
                MultiCheckboxState.On      => Widgets.CheckboxOnTex,
                MultiCheckboxState.Off     => Widgets.CheckboxOffTex,
                MultiCheckboxState.Partial => Widgets.CheckboxPartialTex,
                _                          => BaseContent.ClearTex
            });
            Text.Anchor = anchor;
        }

        listing.Gap(listing.verticalSpacing);
    }
}