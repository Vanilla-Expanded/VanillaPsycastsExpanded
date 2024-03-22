namespace VanillaPsycastsExpanded.Harmonist;

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

public class Ability_HealthSwap : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        if (targets[0].Thing is not Pawn source || targets[1].Thing is not Pawn dest) return;
        MoteBetween mote = (MoteBetween)ThingMaker.MakeThing(VPE_DefOf.VPE_PsycastPsychicEffectTransfer);
        mote.Attach(source, dest);
        mote.Scale         = 1f;
        mote.exactPosition = source.DrawPos;
        GenSpawn.Spawn(mote, source.Position, source.MapHeld);

        List<Hediff> sourceToDest = source.health.hediffSet.hediffs.Where(ShouldTransfer).ToList();
        List<Hediff> destToSource = dest.health.hediffSet.hediffs.Where(ShouldTransfer).ToList();

        foreach (Hediff hediff in sourceToDest) source.health.RemoveHediff(hediff);

        foreach (Hediff hediff in destToSource) dest.health.RemoveHediff(hediff);

        AddAll(source, destToSource);
        AddAll(dest,   sourceToDest);
    }

    private static bool ShouldTransfer(Hediff hediff) => hediff is Hediff_Injury or Hediff_MissingPart or Hediff_Addiction || hediff.def.tendable ||
                                                         hediff.def.makesSickThought || hediff.def.HasComp(typeof(HediffComp_Immunizable));

    private static void AddAll(Pawn pawn, List<Hediff> hediffs)
    {
        void TryAdd()
        {
            hediffs.RemoveAll(hediff =>
            {
                if (pawn.health.hediffSet.PartIsMissing(hediff.Part)) return false;
                try
                {
                    pawn.health.AddHediff(hediff, hediff.Part);
                    return true;
                }
                catch (Exception e)
                {
                    Log.Error($"Error while swapping: {e}");
                    return false;
                }
            });
        }

        TryAdd();
        TryAdd();
    }
}