namespace VanillaPsycastsExpanded.Wildspeaker;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Ability = VFECore.Abilities.Ability;

public class Hediff_Essence : HediffWithComps
{
    public Pawn EssenceOf;

    public override string Label => base.Label + " " + this.EssenceOf.NameShortColored;

    public override bool ShouldRemove => this.EssenceOf == null || (this.EssenceOf.Dead && this.EssenceOf.Corpse is not { Spawned: true });

    public override void Tick()
    {
        base.Tick();
        if (this.EssenceOf is { Dead: true, Corpse: { Spawned: true } } && this.pawn.CurJobDef != VPE_DefOf.VPE_EssenceTransfer)
        {
            Job job = JobMaker.MakeJob(VPE_DefOf.VPE_EssenceTransfer, this.EssenceOf.Corpse);
            job.forceSleep = true;
            this.pawn.jobs.StartJob(job, JobCondition.InterruptForced);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref this.EssenceOf, "essenceOf");
    }
}

public class Ability_EssenceTransfer : Ability
{
    private Pawn curTarget;

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        if (targets[0].Thing is not Pawn human || targets[1].Thing is not Pawn animal) return;
        if (this.curTarget is { Dead: false, Discarded: false, Destroyed: false })
            foreach (Hediff_Essence hediffEssence in this.curTarget.health.hediffSet.hediffs.OfType<Hediff_Essence>().ToList())
                this.curTarget.health.RemoveHediff(hediffEssence);
        else this.curTarget = null;
        Hediff_Essence hediff = (Hediff_Essence)HediffMaker.MakeHediff(VPE_DefOf.VPE_Essence, animal);
        hediff.EssenceOf = human;
        animal.health.AddHediff(hediff);
        this.curTarget = animal;
    }

    public override float GetRangeForPawn()
    {
        if (this.currentTargetingIndex == 1) return 99999;
        return base.GetRangeForPawn();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref this.curTarget, nameof(this.curTarget));
    }
}

public class JobDriver_EssenceTransfer : JobDriver
{
    private         int  restStartTick;
    public override bool TryMakePreToilReservations(bool errorOnFailed) => this.pawn.Reserve(this.job.targetA, this.job, errorOnFailed: errorOnFailed);

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOn(() => this.TargetA.Thing is not Corpse);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        Toil toil = Toils_LayDown.LayDown(TargetIndex.B, false, false, true, false);
        toil.AddPreInitAction(delegate { this.restStartTick = Find.TickManager.TicksGame; });
        toil.AddPreTickAction(delegate
        {
            if (Find.TickManager.TicksGame >= this.restStartTick + GenDate.TicksPerHour * 6) this.ReadyForNextToil();
        });
        yield return toil;
        yield return Toils_General.Do(delegate
        {
            ResurrectionUtility.Resurrect((this.TargetA.Thing as Corpse).InnerPawn);
            this.pawn.Kill(null);
        });
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref this.restStartTick, nameof(this.restStartTick));
    }
}