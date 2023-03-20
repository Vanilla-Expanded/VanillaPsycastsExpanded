namespace VanillaPsycastsExpanded;

using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Skipmaster;
using UnityEngine;
using Verse;

public class PsycastsMod : Mod
{
    public static  Harmony                                Harm;
    public static  PsycastSettings                        Settings;
    private static BackCompatibilityConverter_Psytrainers psytrainerConverter;
    public PsycastsMod(ModContentPack content) : base(content)
    {
        Harm     = new Harmony("OskarPotocki.VanillaPsycastsExpanded");
        Settings = this.GetSettings<PsycastSettings>();
        Harm.Patch(AccessTools.Method(typeof(ThingDefGenerator_Neurotrainer), nameof(ThingDefGenerator_Neurotrainer.ImpliedThingDefs)),
                   postfix: new HarmonyMethod(typeof(ThingDefGenerator_Neurotrainer_ImpliedThingDefs_Patch),
                                              nameof(ThingDefGenerator_Neurotrainer_ImpliedThingDefs_Patch.Postfix)));
        Harm.Patch(AccessTools.Method(typeof(GenDefDatabase), nameof(GenDefDatabase.GetDef)),           new HarmonyMethod(this.GetType(), nameof(PreGetDef)));
        Harm.Patch(AccessTools.Method(typeof(GenDefDatabase), nameof(GenDefDatabase.GetDefSilentFail)), new HarmonyMethod(this.GetType(), nameof(PreGetDef)));
        List<BackCompatibilityConverter> conversionChain =
            (List<BackCompatibilityConverter>)AccessTools.Field(typeof(BackCompatibility), "conversionChain").GetValue(null);
        conversionChain.Add(psytrainerConverter = new BackCompatibilityConverter_Psytrainers());
        conversionChain.Add(new BackCompatibilityConverter_Constructs());
        if (ModsConfig.IsActive("GhostRolly.Rim73"))
            Log.Warning(
                "Vanilla Psycasts Expanded detected Rim73 mod. The mod is throttling hediff ticking which breaks psycast hediffs. You can turn off Rim73 hediff optimization in its mod settings to ensure proper work of Vanilla Psycasts Expanded.");
    }

    public override string SettingsCategory() => "VanillaPsycastsExpanded".Translate();

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
        listing.End();
    }

    public static void PreGetDef(Type __0, ref string __1, bool __2)
    {
        if (__2 && psytrainerConverter.BackCompatibleDefName(__0, __1) is { } newDefName) __1 = newDefName;
    }
}

public class PsycastSettings : ModSettings
{
    public float              XPPerPercent            = 1f;
    public float              baseSpawnChance         = 0.1f;
    public float              additionalAbilityChance = 0.1f;
    public MultiCheckboxState smallMode               = MultiCheckboxState.Partial;
    public bool               shrink                  = true;
    public bool               muteSkipdoor;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref this.XPPerPercent,            "xpPerPercent",                       1f);
        Scribe_Values.Look(ref this.baseSpawnChance,         nameof(this.baseSpawnChance),         0.1f);
        Scribe_Values.Look(ref this.additionalAbilityChance, nameof(this.additionalAbilityChance), 0.1f);
        Scribe_Values.Look(ref this.shrink,                  nameof(this.shrink),                  true);
        Scribe_Values.Look(ref this.muteSkipdoor,            nameof(this.muteSkipdoor));
        Scribe_Values.Look(ref this.smallMode,               nameof(this.smallMode), MultiCheckboxState.Partial);
    }
}