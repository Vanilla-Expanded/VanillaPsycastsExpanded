using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using AbilityDef = VFECore.Abilities.AbilityDef;

namespace VanillaPsycastsExpanded;

public class ThingDefGenerator_Neurotrainer_ImpliedThingDefs_Patch
{
    public static Func<string, bool, ThingDef> BaseNeurotrainer =
        AccessTools.Method(typeof(ThingDefGenerator_Neurotrainer), "BaseNeurotrainer").CreateDelegate<Func<string, bool, ThingDef>>();

    public static void Postfix(ref IEnumerable<ThingDef> __result, bool hotReload)
    {
        __result = __result.Where(def => !def.defName.StartsWith(ThingDefGenerator_Neurotrainer.PsytrainerDefPrefix)).Concat(ImpliedThingDefs(hotReload));
    }

    public static IEnumerable<ThingDef> ImpliedThingDefs(bool hotReload)
    {
        foreach (var abilityDef in DefDatabase<AbilityDef>.AllDefs)
            if (abilityDef.Psycast() is { } psycastExt)
            {
                var thingDef = BaseNeurotrainer(ThingDefGenerator_Neurotrainer.PsytrainerDefPrefix + "_" + abilityDef.defName, hotReload);
                thingDef.label = "PsycastNeurotrainerLabel".Translate(abilityDef.label);
                thingDef.description =
                    "PsycastNeurotrainerDescription".Translate(abilityDef.Named("PSYCAST"),
                        $"[{psycastExt.path.LabelCap}]\n{abilityDef.description}".Named("PSYCASTDESCRIPTION"));
                thingDef.comps.Add(new CompProperties_Usable
                {
                    compClass = typeof(CompUsable),
                    useJob = JobDefOf.UseNeurotrainer,
                    useLabel = "PsycastNeurotrainerUseLabel".Translate(abilityDef.label)
                });
                thingDef.comps.Add(new CompProperties_UseEffect_Psytrainer
                {
                    ability = abilityDef
                });
                thingDef.statBases.Add(new()
                {
                    stat = StatDefOf.MarketValue,
                    value = Mathf.Round(500f + 300f * psycastExt.level)
                });
                thingDef.thingCategories = new()
                {
                    ThingCategoryDefOf.NeurotrainersPsycast
                };
                thingDef.thingSetMakerTags = new()
                {
                    "RewardStandardLowFreq"
                };
                thingDef.modContentPack = abilityDef.modContentPack;
                thingDef.descriptionHyperlinks = new()
                {
                    new(abilityDef)
                };
                thingDef.stackLimit = 1;
                yield return thingDef;
            }
    }
}

public class CompProperties_UseEffect_Psytrainer : CompProperties_UseEffectGiveAbility
{
    public CompProperties_UseEffect_Psytrainer() => compClass = typeof(CompPsytrainer);
}

public class CompPsytrainer : CompUseEffect_GiveAbility
{
    public override void DoEffect(Pawn usedBy)
    {
        if (Props.ability?.Psycast()?.path is { } path && usedBy.Psycasts() is { } psycasts
                                                       && !psycasts.unlockedPaths.Contains(path))
            psycasts.UnlockPath(path);
        base.DoEffect(usedBy);
    }

    public override AcceptanceReport CanBeUsedBy(Pawn p)
    {
        if (p.Psycasts() is null or { level: <= 0 }) return "VPE.MustBePsycaster".Translate();

        var path = Props.ability?.Psycast()?.path;
        if (path != null)
            if (path.CanPawnUnlock(p) is false)
                if (path.ignoreLockRestrictionsForNeurotrainers is false)
                    return Props.ability.Psycast().path.lockedReason;

        if (p.GetComp<CompAbilities>().HasAbility(Props.ability)) return "VPE.AlreadyHasPsycast".Translate(Props.ability.LabelCap);

        return true;
    }
}
