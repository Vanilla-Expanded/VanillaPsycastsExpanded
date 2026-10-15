using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Ability = VEF.Abilities.Ability;

namespace VanillaPsycastsExpanded.Wildspeaker;

public class Hediff_Essence : HediffWithComps
{
    public Pawn EssenceOf;

    public override string Label => base.Label + " " + EssenceOf.NameShortColored;

    public override bool ShouldRemove => EssenceOf == null || (EssenceOf.Dead && EssenceOf.Corpse is not { Spawned: true });

    public override void Tick()
    {
        base.Tick();
        if (EssenceOf is { Dead: true, Corpse: { Spawned: true } } && pawn.CurJobDef != VPE_DefOf.VPE_EssenceTransfer)
        {
            var job = JobMaker.MakeJob(VPE_DefOf.VPE_EssenceTransfer, EssenceOf.Corpse);
            job.forceSleep = true;
            pawn.jobs.StartJob(job, JobCondition.InterruptForced);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref EssenceOf, "essenceOf");
    }
}

public class Ability_EssenceTransfer : Ability
{
    private Pawn curTarget;

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        if (targets[0].Thing is not Pawn human || targets[1].Thing is not Pawn animal) return;
        if (curTarget is { Dead: false, Discarded: false, Destroyed: false })
            foreach (var hediffEssence in curTarget.health.hediffSet.hediffs.OfType<Hediff_Essence>().ToList())
                curTarget.health.RemoveHediff(hediffEssence);
        else curTarget = null;
        var hediff = (Hediff_Essence)HediffMaker.MakeHediff(VPE_DefOf.VPE_Essence, animal);
        hediff.EssenceOf = human;
        animal.health.AddHediff(hediff);
        curTarget = animal;
    }

    public override float GetRangeForPawn()
    {
        if (currentTargetingIndex == 1) return 99999;
        return base.GetRangeForPawn();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref curTarget, nameof(curTarget));
    }
}

public class JobDriver_EssenceTransfer : JobDriver
{
    private int restStartTick;
    public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed);

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOn(() => TargetA.Thing is not Corpse);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        var toil = Toils_LayDown.LayDown(TargetIndex.B, false, false, true, false);
        toil.AddPreInitAction(delegate { restStartTick = Find.TickManager.TicksGame; });
        toil.AddPreTickAction(delegate
        {
            if (Find.TickManager.TicksGame >= restStartTick + GenDate.TicksPerHour * 6) ReadyForNextToil();
        });
        yield return toil;
        yield return Toils_General.Do(delegate
        {
            ResurrectionUtility.TryResurrect((TargetA.Thing as Corpse).InnerPawn);
            pawn.Kill(null);
        });
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref restStartTick, nameof(restStartTick));
    }
}
