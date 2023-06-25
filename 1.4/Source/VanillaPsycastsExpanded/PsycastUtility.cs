using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

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

    public static void RecheckPaths(this Pawn pawn)
    {
        var psycasts = pawn.Psycasts();
        if (psycasts != null)
        {
            if (psycasts.unlockedPaths != null)
                foreach (var path in psycasts.unlockedPaths.ToList())
                    if (path.ensureLockRequirement)
                        if (path.CanPawnUnlock(pawn) is false)
                        {
                            psycasts.previousUnlockedPaths.Add(path);
                            psycasts.unlockedPaths.Remove(path);
                        }

            if (psycasts.previousUnlockedPaths != null)
                foreach (var path in psycasts.previousUnlockedPaths.ToList())
                    if (path.ensureLockRequirement)
                    {
                        if (path.CanPawnUnlock(pawn))
                        {
                            psycasts.previousUnlockedPaths.Remove(path);
                            psycasts.unlockedPaths.Add(path);
                        }
                    }
                    else
                    {
                        psycasts.previousUnlockedPaths.Remove(path);
                        psycasts.unlockedPaths.Add(path);
                    }
        }
    }

    public static bool IsEltexOrHasEltexMaterial(this ThingDef def)
    {
        return def != null &&
               (def == VPE_DefOf.VPE_Eltex ||
                (def.costList != null && def.costList.Any(x => x.thingDef == VPE_DefOf.VPE_Eltex)) ||
                eltexThings.Contains(def));
    }

    public static Hediff_PsycastAbilities Psycasts(this Pawn pawn) =>
        (Hediff_PsycastAbilities)pawn?.health?.hediffSet?.GetFirstHediffOfDef(VPE_DefOf.VPE_PsycastAbilityImplant);

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

        if (pawn.RaceProps.IsFlesh)
        {
            hypothermiaHediff = HediffDefOf.Hypothermia;
            return true;
        }

        hypothermiaHediff = null;
        return false;
    }
}
