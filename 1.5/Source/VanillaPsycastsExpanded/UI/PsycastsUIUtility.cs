namespace VanillaPsycastsExpanded.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using VFECore.Abilities;
    using VFECore.UItils;
    using AbilityDef = VFECore.Abilities.AbilityDef;

    [StaticConstructorOnStartup]
    public static class PsycastsUIUtility
    {
        private static readonly Dictionary<MeditationFocusDef, Texture2D> meditationIcons;

        private static readonly float[][] abilityTreeXOffsets =
        {
            new[] {-18f},
            new[] {-11f - 36f, 11f},
            new[] {-18f - 15f - 36f, -18f, 18f + 15f}
        };

        public static Hediff_PsycastAbilities Hediff;
        public static CompAbilities           CompAbilities;

        static PsycastsUIUtility()
        {
            meditationIcons = new Dictionary<MeditationFocusDef, Texture2D>();
            foreach (MeditationFocusDef def in DefDatabase<MeditationFocusDef>.AllDefs)
            {
                MeditationFocusExtension ext = def.GetModExtension<MeditationFocusExtension>();
                if (ext is null)
                {
                    Log.Warning(
                        $"MeditationFocusDef {def} does not have a MeditationFocusExtension, which means it will not have an icon in the Psycasts UI.\nPlease ask {def.modContentPack.ModMetaData.AuthorsString} to add one.");
                    meditationIcons.Add(def, BaseContent.WhiteTex);
                }
                else
                {
                    meditationIcons.Add(def, ContentFinder<Texture2D>.Get(ext.icon));

                    if (ext.statParts.NullOrEmpty()) continue;
                    foreach (StatPart_Focus statPart in ext.statParts)
                    {
                        statPart.focus                      =   def;
                        statPart.parentStat                 =   StatDefOf.MeditationFocusStrength;
                        StatDefOf.MeditationFocusGain.parts ??= new List<StatPart>();
                        StatDefOf.MeditationFocusGain.parts.Add(statPart);
                    }
                }
            }
        }

        public static void LabelWithIcon(this Listing_Standard listing, Texture2D icon, string label)
        {
            float height = Text.CalcHeight(label, listing.ColumnWidth);
            Rect  rect   = listing.GetRect(height);
            float width  = icon.width * (height / icon.height);
            GUI.DrawTexture(rect.TakeLeftPart(width), icon);
            rect.xMin += 3f;
            Widgets.Label(rect, label);
            listing.Gap(3f);
        }

        public static void StatDisplay(this Listing_Standard listing, Texture2D icon, StatDef stat, Thing thing)
        {
            listing.LabelWithIcon(
                icon,
                stat.LabelCap + ": " + stat.Worker.GetStatDrawEntryLabel(stat, thing.GetStatValue(stat), stat.toStringNumberSense, StatRequest.For(thing)));
        }

        public static Rect CenterRect(this Rect rect, Vector2 size) => new(rect.center - size / 2f, size);

        public static Texture2D Icon(this MeditationFocusDef def) => meditationIcons[def];

        public static void DrawPathBackground(ref Rect rect, PsycasterPathDef def, bool altTex = false)
        {
            Texture2D texture = altTex ? def.backgroundImage : def.altBackgroundImage;
            GUI.color = new ColorInt(97, 108, 122).ToColor;
            Widgets.DrawBox(rect.ExpandedBy(2f), 1, Texture2D.whiteTexture);
            GUI.color = Color.white;
            Rect labelRect = rect.TakeBottomPart(30f);
            Widgets.DrawRectFast(labelRect, Widgets.WindowBGFillColor);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(labelRect, def.LabelCap);
            GUI.DrawTexture(rect, texture);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static bool EnsureInit()
        {
            if (Hediff != null && CompAbilities != null) return true;
            Log.Error("[VPE] PsycastsUIUtility was used without being initialized.");
            return false;
        }

        public static void DoPathAbilities(Rect inRect, PsycasterPathDef path, Dictionary<AbilityDef, Vector2> abilityPos, Action<Rect, AbilityDef> doAbility)
        {
            if (!EnsureInit()) return;
            foreach (AbilityDef def in path.abilities)
                if (def.Psycast()?.prerequisites is { } prerequisites && abilityPos.ContainsKey(def))
                    foreach (AbilityDef abilityDef in prerequisites.Where(abilityDef => abilityPos.ContainsKey(abilityDef)))
                        Widgets.DrawLine(abilityPos[def], abilityPos[abilityDef], CompAbilities.HasAbility(abilityDef) ? Color.white : Color.grey, 2f);

            for (int level = 0; level < path.abilityLevelsInOrder.Length; level++)
            {
                Rect levelRect = new(inRect.x, inRect.y + (path.MaxLevel - 1 - level) * inRect.height / path.MaxLevel + 10f, inRect.width, inRect.height / 5f);
                AbilityDef[] abilities = path.abilityLevelsInOrder[level];
                for (int pos = 0; pos < abilities.Length; pos++)
                {
                    Rect       rect = new(levelRect.x + levelRect.width / 2 + abilityTreeXOffsets[abilities.Length - 1][pos], levelRect.y, 36f, 36f);
                    AbilityDef def  = abilities[pos];
                    if (def == PsycasterPathDef.Blank) continue;
                    abilityPos[def] = rect.center;
                    doAbility(rect, def);
                }
            }
        }

        public static void DrawAbility(Rect inRect, AbilityDef ability)
        {
            Color color = Mouse.IsOver(inRect) ? GenUI.MouseoverColor : Color.white;
            MouseoverSounds.DoRegion(inRect, SoundDefOf.Mouseover_Command);
            GUI.color = color;
            GUI.DrawTexture(inRect, Command.BGTexShrunk);
            GUI.color = Color.white;
            GUI.DrawTexture(inRect, ability.icon);
        }
    }
}