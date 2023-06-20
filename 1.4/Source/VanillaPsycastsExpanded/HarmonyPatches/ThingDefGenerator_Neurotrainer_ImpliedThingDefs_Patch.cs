namespace VanillaPsycastsExpanded;

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MonoMod.Utils;
using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using AbilityDef = VFECore.Abilities.AbilityDef;

public class ThingDefGenerator_Neurotrainer_ImpliedThingDefs_Patch
{
    public static Func<ThingDef> BaseNeurotrainer =
        AccessTools.Method(typeof(ThingDefGenerator_Neurotrainer), "BaseNeurotrainer").CreateDelegate<Func<ThingDef>>();

    public static void Postfix(ref IEnumerable<ThingDef> __result)
    {
        __result = __result.Where(def => !def.defName.StartsWith(ThingDefGenerator_Neurotrainer.PsytrainerDefPrefix)).Concat(ImpliedThingDefs());
    }

    public static IEnumerable<ThingDef> ImpliedThingDefs()
    {
        foreach (AbilityDef abilityDef in DefDatabase<AbilityDef>.AllDefs)
            if (abilityDef.Psycast() is { } psycastExt)
            {
                ThingDef thingDef = BaseNeurotrainer();
                thingDef.defName = ThingDefGenerator_Neurotrainer.PsytrainerDefPrefix + "_" + abilityDef.defName;
                thingDef.label   = "PsycastNeurotrainerLabel".Translate(abilityDef.label);
                thingDef.description =
                    "PsycastNeurotrainerDescription".Translate(abilityDef.Named("PSYCAST"),
                                                               $"[{psycastExt.path.LabelCap}]\n{abilityDef.description}".Named("PSYCASTDESCRIPTION"));
                thingDef.comps.Add(new CompProperties_Usable
                {
                    compClass = typeof(CompUsable),
                    useJob    = JobDefOf.UseNeurotrainer,
                    useLabel  = "PsycastNeurotrainerUseLabel".Translate(abilityDef.label)
                });
                thingDef.comps.Add(new CompProperties_UseEffect_Psytrainer
                {
                    ability = abilityDef
                });
                thingDef.statBases.Add(new StatModifier
                {
                    stat  = StatDefOf.MarketValue,
                    value = Mathf.Round(500f + 300f * psycastExt.level)
                });
                thingDef.thingCategories = new List<ThingCategoryDef>
                {
                    ThingCategoryDefOf.NeurotrainersPsycast
                };
                thingDef.thingSetMakerTags = new List<string>
                {
                    "RewardStandardLowFreq"
                };
                thingDef.modContentPack = abilityDef.modContentPack;
                thingDef.descriptionHyperlinks = new List<DefHyperlink>
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
    public CompProperties_UseEffect_Psytrainer() => this.compClass = typeof(CompPsytrainer);
}

public class CompPsytrainer : CompUseEffect_GiveAbility
{
    public override void DoEffect(Pawn usedBy)
    {
        if (this.Props.ability?.Psycast()?.path is { } path && usedBy.Psycasts() is { } psycasts 
            && !psycasts.unlockedPaths.Contains(path))
            psycasts.UnlockPath(path);
        base.DoEffect(usedBy);
    }

    public override bool CanBeUsedBy(Pawn p, out string failReason)
    {
        if (p.Psycasts() is null or { level: <= 0 })
        {
            failReason = "VPE.MustBePsycaster".Translate();
            return false;
        }
        var path = this.Props.ability?.Psycast()?.path;
        if (path != null)
        {
            if (path.CanPawnUnlock(p) is false)
            {
                if (path.ignoreLockRestrictionsForNeurotrainers is false)
                {
                    failReason = this.Props.ability.Psycast().path.lockedReason;
                    return false;
                }
            }
        }
        if (p.GetComp<CompAbilities>().HasAbility(this.Props.ability))
        {
            failReason = "VPE.AlreadyHasPsycast".Translate(this.Props.ability.LabelCap);
            return false;
        }
        failReason = null;
        return true;
    }
}