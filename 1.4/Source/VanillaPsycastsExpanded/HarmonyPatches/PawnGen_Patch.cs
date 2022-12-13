namespace VanillaPsycastsExpanded;

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using VFECore.Abilities;
using AbilityDef = VFECore.Abilities.AbilityDef;

[HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
[HarmonyAfter("OskarPotocki.VFECore")]
public class PawnGen_Patch
{
    [HarmonyPostfix]
    public static void Postfix(Pawn __result)
    {
        if (__result == null) return;

        PawnKindAbilityExtension_Psycasts psycastExtension = __result.kindDef.GetModExtension<PawnKindAbilityExtension_Psycasts>();

        CompAbilities comp = null;

        if (psycastExtension != null)
        {
            comp = __result.GetComp<CompAbilities>();

            if (psycastExtension.implantDef != null)
            {
                Hediff_Psylink psylink = __result.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicAmplifier) as Hediff_Psylink;

                if (psylink == null)
                {
                    psylink = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, __result, __result.health.hediffSet.GetBrain()) as Hediff_Psylink;
                    __result.health.AddHediff(psylink);
                }


                Hediff_PsycastAbilities implant = (__result.health.hediffSet.GetFirstHediffOfDef(psycastExtension.implantDef) as Hediff_PsycastAbilities)!;

                if (implant.psylink == null)
                    implant.InitializeFromPsylink(psylink);

                foreach (PathUnlockData unlockedPath in psycastExtension.unlockedPaths)
                    if (unlockedPath.path.CanPawnUnlock(__result))
                    {
                        implant.UnlockPath(unlockedPath.path);

                        int abilityCount = unlockedPath.unlockedAbilityCount.RandomInRange;


                        IEnumerable<AbilityDef> abilitySelection = new List<AbilityDef>();

                        for (int level = unlockedPath.unlockedAbilityLevelRange.min;
                             level < unlockedPath.unlockedAbilityLevelRange.max && level < unlockedPath.path.MaxLevel;
                             level++)
                            abilitySelection = abilitySelection.Concat(unlockedPath.path.abilityLevelsInOrder[level-1].Except(PsycasterPathDef.Blank));

                        List<AbilityDef> abilitySelectionList = abilitySelection.ToList();
                        List<AbilityDef> abilitySelectionListFiltered;

                        while ((abilitySelectionListFiltered = abilitySelectionList.Where(ab => ab.Psycast().PrereqsCompleted(comp)).ToList()).Any() &&
                               abilityCount > 0)
                        {
                            abilityCount--;
                            AbilityDef abilityDef = abilitySelectionListFiltered.RandomElement();
                            comp.GiveAbility(abilityDef);

                            implant.ChangeLevel(1, false);
                            implant.points--;

                            abilitySelectionList.Remove(abilityDef);
                        }
                    }

                int statCount = psycastExtension.statUpgradePoints.RandomInRange;
                implant.ChangeLevel(statCount);
                implant.points -= statCount;
                implant.ImproveStats(statCount);
            }
        }

        if (Find.Storyteller?.def == VPE_DefOf.VPE_Basilicus && __result.RaceProps.intelligence >= Intelligence.Humanlike)
            if (Rand.Value < PsycastsMod.Settings.baseSpawnChance)
            {
                Hediff_Psylink psylink = __result.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicAmplifier) as Hediff_Psylink;

                if (psylink == null)
                {
                    psylink = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, __result, __result.health.hediffSet.GetBrain()) as Hediff_Psylink;
                    __result.health.AddHediff(psylink);
                }

                Hediff_PsycastAbilities implant =
                    __result.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_PsycastAbilityImplant) as Hediff_PsycastAbilities ??
                    HediffMaker.MakeHediff(VPE_DefOf.VPE_PsycastAbilityImplant, __result,
                                           __result.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Brain).FirstOrFallback()) as Hediff_PsycastAbilities;

                if (implant.psylink == null)
                    implant.InitializeFromPsylink(psylink);

                PsycasterPathDef path = DefDatabase<PsycasterPathDef>.AllDefsListForReading.Where(ppd => ppd.CanPawnUnlock(__result)).RandomElement();
                implant.UnlockPath(path);

                comp ??= __result.GetComp<CompAbilities>();

                IEnumerable<AbilityDef> abilities = path.abilities.Except(comp.LearnedAbilities.Select(ab => ab.def));

                do
                {
                    if (abilities.Where(ab => ab.GetModExtension<AbilityExtension_Psycast>().PrereqsCompleted(comp)).TryRandomElement(out AbilityDef ab))
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