﻿namespace VanillaPsycastsExpanded;

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

[StaticConstructorOnStartup]
public static class PsycastUtility
{
    private static readonly HashSet<ThingDef> eltexThings;

    static PsycastUtility()
    {
        eltexThings = DefDatabase<RecipeDef>.AllDefs
            .Where(recipe => recipe.ingredients.Any(x => x.IsFixedIngredient && x.FixedIngredient == VPE_DefOf.VPE_Eltex))
            .Select(recipe => recipe.ProducedThingDef)
            .ToHashSet();
    }

    public static bool IsEltexOrHasEltexMaterial(this ThingDef def)
    {
        return def != null &&
            (def == VPE_DefOf.VPE_Eltex ||
            (def.costList != null && def.costList.Any(x => x.thingDef == VPE_DefOf.VPE_Eltex)) ||
            eltexThings.Contains(def));
    }

    public static Hediff_PsycastAbilities Psycasts(this Pawn pawn) =>
        (Hediff_PsycastAbilities)pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_PsycastAbilityImplant);

    [DebugAction("Pawns", "Reset Psycasts", true, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void ResetPsycasts(Pawn p)
    {
        p.Psycasts()?.Reset();
    }

    public static bool CanReceiveHypothermia(this Pawn pawn, out HediffDef hypothermiaHediff)
    {
        if (pawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid)
        {
            hypothermiaHediff = VPE_DefOf.HypothermicSlowdown;
            return true;
        }
        else if (pawn.RaceProps.IsFlesh)
        {
            hypothermiaHediff = HediffDefOf.Hypothermia;
            return true;
        }
        hypothermiaHediff = null;
        return false;
    }
}