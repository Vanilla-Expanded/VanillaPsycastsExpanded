namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using VFECore.Abilities;
    public class Hediff_RegrowLimbs : HediffWithComps
    {
        public override void PostTick()
        {
            base.PostTick();
            if (Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
            {
                bool healedOnce = false;
                var injuredHediffs = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().ToList();
                if (injuredHediffs.Any())
                {
                    injuredHediffs.RandomElement().Heal(1f);
                    healedOnce = true;
                }
                else
                {
                    var nonMissingParts = pawn.health.hediffSet.GetNotMissingParts().ToList();
                    var missingParts = pawn.def.race.body.AllParts.Where(x => pawn.health.hediffSet.PartIsMissing(x) 
                        && nonMissingParts.Contains(x.parent) && !pawn.health.hediffSet.AncestorHasDirectlyAddedParts(x)).ToList();
                    if (missingParts.Any())
                    {
                        var missingPart = missingParts.RandomElement();
                        var currentMissingHediffs = pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().ToList();
                        pawn.health.RestorePart(missingPart);
                        var currentMissingHediffs2 = pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().ToList();
                        var removedMissingPartHediff = currentMissingHediffs.Where(x => !currentMissingHediffs2.Contains(x));
                        foreach (var missingPartHediff in removedMissingPartHediff)
                        {
                            var regeneratingHediff = HediffMaker.MakeHediff(VPE_DefOf.VPE_Regenerating, pawn, missingPartHediff.Part);
                            regeneratingHediff.Severity = missingPartHediff.Part.def.GetMaxHealth(pawn) - 1;
                            pawn.health.AddHediff(regeneratingHediff);
                        }
                        healedOnce = true;
                    }
                }
                if (healedOnce)
                {
                    FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.HealingCross);
                }
            }
        }
    }
}