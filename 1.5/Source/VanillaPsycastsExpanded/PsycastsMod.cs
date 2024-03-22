using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaPsycastsExpanded;

public class PsycastsMod : Mod
{
    public static Harmony Harm;
    public static PsycastSettings Settings;
    private static BackCompatibilityConverter_Psytrainers psytrainerConverter;

    public PsycastsMod(ModContentPack content) : base(content)
    {
        Harm = new("OskarPotocki.VanillaPsycastsExpanded");
        Settings = GetSettings<PsycastSettings>();
        Harm.Patch(AccessTools.Method(typeof(ThingDefGenerator_Neurotrainer), nameof(ThingDefGenerator_Neurotrainer.ImpliedThingDefs)),
            postfix: new(typeof(ThingDefGenerator_Neurotrainer_ImpliedThingDefs_Patch),
                nameof(ThingDefGenerator_Neurotrainer_ImpliedThingDefs_Patch.Postfix)));
        Harm.Patch(AccessTools.Method(typeof(GenDefDatabase), nameof(GenDefDatabase.GetDef)), new(GetType(), nameof(PreGetDef)));
        Harm.Patch(AccessTools.Method(typeof(GenDefDatabase), nameof(GenDefDatabase.GetDefSilentFail)), new(GetType(), nameof(PreGetDef)));
        var conversionChain =
            (List<BackCompatibilityConverter>)AccessTools.Field(typeof(BackCompatibility), "conversionChain").GetValue(null);
        conversionChain.Add(psytrainerConverter = new());
        conversionChain.Add(new BackCompatibilityConverter_Constructs());
        if (ModsConfig.IsActive("GhostRolly.Rim73") || ModsConfig.IsActive("GhostRolly.Rim73_steam"))
            Log.Warning(
                "Vanilla Psycasts Expanded detected Rim73 mod. The mod is throttling hediff ticking which breaks psycast hediffs. You can turn off Rim73 hediff optimization in its mod settings to ensure proper work of Vanilla Psycasts Expanded.");

        LongEventHandler.ExecuteWhenFinished(ApplySettings);
    }

    public override string SettingsCategory() => "VanillaPsycastsExpanded".Translate();

    public override void WriteSettings()
    {
        base.WriteSettings();
        ApplySettings();
    }

    private void ApplySettings()
    {
        HediffDefOf.PsychicAmplifier.maxSeverity = Settings.maxLevel;
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        Listing_Standard listing = new();
        listing.Begin(inRect);
        listing.Label("VPE.XPPerPercent".Translate() + ": " + Settings.XPPerPercent);
        Settings.XPPerPercent = listing.Slider(Settings.XPPerPercent, 0.1f, 10f);
        listing.Label("VPE.PsycasterSpawnBaseChance".Translate() + ": " + Settings.baseSpawnChance * 100 + "%");
        Settings.baseSpawnChance = listing.Slider(Settings.baseSpawnChance, 0, 1f);
        listing.Label("VPE.PsycasterSpawnAdditional".Translate() + ": " + Settings.additionalAbilityChance * 100 + "%");
        Settings.additionalAbilityChance = listing.Slider(Settings.additionalAbilityChance, 0, 1f);
        listing.CheckboxLabeled("VPE.AllowShrink".Translate(), ref Settings.shrink, "VPE.AllowShrink.Desc".Translate());
        listing.CheckboxMultiLabeled("VPE.SmallMode".Translate(), ref Settings.smallMode, "VPE.SmallMode.Desc".Translate());
        listing.CheckboxLabeled("VPE.MuteSkipdoor".Translate(), ref Settings.muteSkipdoor);
        listing.Label("VPE.MaxLevel".Translate() + ": " + Settings.maxLevel);
        Settings.maxLevel = (int)listing.Slider(Settings.maxLevel, 1, 300);
        listing.CheckboxLabeled("VPE.ChangeFocusGain".Translate(), ref Settings.changeFocusGain, "VPE.ChangeFocusGain.Desc".Translate());
        listing.End();
    }

    public static void PreGetDef(Type __0, ref string __1, bool __2)
    {
        if (__2 && psytrainerConverter.BackCompatibleDefName(__0, __1) is { } newDefName) __1 = newDefName;
    }
}

public class PsycastSettings : ModSettings
{
    public float additionalAbilityChance = 0.1f;
    public float baseSpawnChance = 0.1f;
    public bool changeFocusGain;
    public int maxLevel = 30;
    public bool muteSkipdoor;
    public bool shrink = true;
    public MultiCheckboxState smallMode = MultiCheckboxState.Partial;
    public float XPPerPercent = 1f;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref XPPerPercent, "xpPerPercent", 1f);
        Scribe_Values.Look(ref baseSpawnChance, nameof(baseSpawnChance), 0.1f);
        Scribe_Values.Look(ref additionalAbilityChance, nameof(additionalAbilityChance), 0.1f);
        Scribe_Values.Look(ref shrink, nameof(shrink), true);
        Scribe_Values.Look(ref muteSkipdoor, nameof(muteSkipdoor));
        Scribe_Values.Look(ref smallMode, nameof(smallMode), MultiCheckboxState.Partial);
        Scribe_Values.Look(ref maxLevel, nameof(maxLevel), 30);
        Scribe_Values.Look(ref changeFocusGain, nameof(changeFocusGain));
    }
}
