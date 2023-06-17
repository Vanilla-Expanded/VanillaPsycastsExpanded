using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using VFECore.Abilities;
using AbilityDef = VFECore.Abilities.AbilityDef;

namespace VanillaPsycastsExpanded;

[HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
[HarmonyAfter("OskarPotocki.VFECore")]
public class PawnGen_Patch
{
    [HarmonyPostfix]
    public static void Postfix(Pawn __result, PawnGenerationRequest request)
    {
        if (__result == null || request.AllowedDevelopmentalStages.Newborn()) return;

        var psycastExtension = __result.kindDef.GetModExtension<PawnKindAbilityExtension_Psycasts>();

        CompAbilities comp = null;

        if (psycastExtension != null)
        {
            comp = __result.GetComp<CompAbilities>();

            if (psycastExtension.implantDef != null)
            {
                var psylink = __result.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicAmplifier) as Hediff_Psylink;

                if (psylink == null)
                {
                    psylink = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, __result, __result.health.hediffSet.GetBrain()) as Hediff_Psylink;
                    __result.health.AddHediff(psylink);
                }


                var implant = (__result.health.hediffSet.GetFirstHediffOfDef(psycastExtension.implantDef) as Hediff_PsycastAbilities)!;

                if (implant.psylink == null)
                    implant.InitializeFromPsylink(psylink);

                foreach (var unlockedPath in psycastExtension.unlockedPaths)
                    if (unlockedPath.path.CanPawnUnlock(__result))
                    {
                        implant.UnlockPath(unlockedPath.path);

                        var abilityCount = unlockedPath.unlockedAbilityCount.RandomInRange;


                        IEnumerable<AbilityDef> abilitySelection = new List<AbilityDef>();

                        for (var level = unlockedPath.unlockedAbilityLevelRange.min;
                             level < unlockedPath.unlockedAbilityLevelRange.max && level < unlockedPath.path.MaxLevel;
                             level++)
                            abilitySelection = abilitySelection.Concat(unlockedPath.path.abilityLevelsInOrder[level - 1].Except(PsycasterPathDef.Blank));

                        var abilitySelectionList = abilitySelection.ToList();
                        List<AbilityDef> abilitySelectionListFiltered;

                        while ((abilitySelectionListFiltered = abilitySelectionList.Where(ab => ab.Psycast().PrereqsCompleted(comp)).ToList()).Any() &&
                               abilityCount > 0)
                        {
                            abilityCount--;
                            var abilityDef = abilitySelectionListFiltered.RandomElement();
                            comp.GiveAbility(abilityDef);

                            implant.ChangeLevel(1, false);
                            implant.points--;

                            abilitySelectionList.Remove(abilityDef);
                        }
                    }

                var statCount = psycastExtension.statUpgradePoints.RandomInRange;
                implant.ChangeLevel(statCount);
                implant.points -= statCount;
                implant.ImproveStats(statCount);
            }
        }

        if (Find.Storyteller?.def == VPE_DefOf.VPE_Basilicus && __result.RaceProps.intelligence >= Intelligence.Humanlike)
            if (Rand.Value < PsycastsMod.Settings.baseSpawnChance)
            {
                var psylink = __result.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicAmplifier) as Hediff_Psylink;

                if (psylink == null)
                {
                    psylink = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, __result, __result.health.hediffSet.GetBrain()) as Hediff_Psylink;
                    __result.health.AddHediff(psylink);
                }

                var implant =
                    __result.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_PsycastAbilityImplant) as Hediff_PsycastAbilities ??
                    HediffMaker.MakeHediff(VPE_DefOf.VPE_PsycastAbilityImplant, __result,
                        __result.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Brain).FirstOrFallback()) as Hediff_PsycastAbilities;

                if (implant.psylink == null)
                    implant.InitializeFromPsylink(psylink);

                var path = DefDatabase<PsycasterPathDef>.AllDefsListForReading.Where(ppd => ppd.CanPawnUnlock(__result)).RandomElement();
                implant.UnlockPath(path);

                comp ??= __result.GetComp<CompAbilities>();

                var abilities = path.abilities.Except(comp.LearnedAbilities.Select(ab => ab.def));

                do
                {
                    if (abilities.Where(ab => ab.GetModExtension<AbilityExtension_Psycast>().PrereqsCompleted(comp)).TryRandomElement(out var ab))
                    {
                        comp.GiveAbility(ab);
                        if (implant.points <= 0)
                            implant.ChangeLevel(1, false);
                        implant.points--;
                        abilities = abilities.Except(ab);
                    }
                    else
                        break;
                } while (Rand.Value < PsycastsMod.Settings.additionalAbilityChance);
            }
    }
}
