namespace VanillaPsycastsExpanded.Skipmaster;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.Sound;
using Ability = VFECore.Abilities.Ability;

public class Dialog_RenameSkipdoor : Dialog_Rename
{
    public Skipdoor Skipdoor;

    public Dialog_RenameSkipdoor(Skipdoor skipdoor)
    {
        this.Skipdoor = skipdoor;
        this.curName  = skipdoor.Name ?? skipdoor.def.label + " #" + Rand.Range(1, 99).ToString("D2");
    }

    protected override void SetName(string name)
    {
        this.Skipdoor.Name = name;
    }
}

public class Ability_Skipdoor : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
        {
            Skipdoor skipdoor = (Skipdoor)ThingMaker.MakeThing(VPE_DefOf.VPE_Skipdoor);
            skipdoor.Pawn = this.pawn;
            Find.WindowStack.Add(new Dialog_RenameSkipdoor(skipdoor));
            GenSpawn.Spawn(skipdoor, target.Cell, this.pawn.Map);
        }
    }
}

public class WorldComponent_SkipdoorManager : WorldComponent
{
    public static WorldComponent_SkipdoorManager Instance;

    private List<Skipdoor> skipdoors = new();

    public WorldComponent_SkipdoorManager(World world) : base(world) => Instance = this;

    public List<Skipdoor> Skipdoors
    {
        get
        {
            this.skipdoors.RemoveAll(skipdoor => skipdoor is not { Spawned: true });
            return this.skipdoors;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref this.skipdoors, "skipdoors", LookMode.Reference);
    }
}

public class JobDriver_UseSkipdoor : JobDriver
{
    private IntVec3  targetCell;
    private Effecter destEffecter;
    public  Skipdoor Origin => this.job.targetA.Thing as Skipdoor;
    public  Skipdoor Dest   => this.job.globalTarget.Thing as Skipdoor;

    public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

    public override string GetReport() =>
        JobUtility.GetResolvedJobReportRaw(this.job.def.reportString, this.Origin.Name, this.Origin, this.Dest.Name, this.Dest, null, null);

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.AddEndCondition(() => this.Dest is null || !this.Dest.Spawned || this.Dest.Destroyed ? JobCondition.Incompletable : JobCondition.Ongoing);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        Toil wait = Toils_General.Wait(16, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A).WithEffect(EffecterDefOf.Skip_Entry, TargetIndex.A);
        wait.AddPreTickAction(() =>
        {
            Map targetMap = this.job.globalTarget.Map;
            if (this.ticksLeftThisToil == 5)
            {
                FleckMaker.Static(this.pawn.Position, this.pawn.Map, FleckDefOf.PsycastSkipFlashEntry);
                FleckMaker.Static(this.targetCell,    targetMap,     FleckDefOf.PsycastSkipInnerExit);
                FleckMaker.Static(this.targetCell,    targetMap,     FleckDefOf.PsycastSkipOuterRingExit);
                SoundDefOf.Psycast_Skip_Entry.PlayOneShot(this.Origin);
                SoundDefOf.Psycast_Skip_Exit.PlayOneShot(this.Dest);
            }
            else if (this.ticksLeftThisToil == 15)
            {
                this.targetCell             = GenAdj.CellsAdjacentCardinal(this.Dest).Where(c => c.Standable(targetMap)).RandomElement();
                this.destEffecter           = EffecterDefOf.Skip_Exit.Spawn(this.targetCell, targetMap);
                this.destEffecter.ticksLeft = 15;
            }

            this.destEffecter?.EffectTick(new TargetInfo(this.targetCell, targetMap), new TargetInfo(this.targetCell, targetMap));
        });
        wait.AddFinishAction(() => { this.destEffecter?.Cleanup(); });
        yield return wait;
        yield return Toils_General.DoAtomic(() =>
        {
            Pawn    localPawn = this.pawn;
            IntVec3 cell      = this.targetCell;
            Map     map       = this.job.globalTarget.Map;
            bool    drafted   = localPawn.Drafted;
            localPawn.teleporting = true;
            localPawn.ClearAllReservations(false);
            localPawn.ExitMap(false, Rot4.Invalid);
            localPawn.teleporting = false;
            GenSpawn.Spawn(localPawn, cell, map);
            localPawn.drafter.Drafted = drafted;
        });
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref this.targetCell, nameof(this.targetCell));
    }
}

public class CaravanArrivalAction_UseSkipdoor : CaravanArrivalAction
{
    public Skipdoor Target;
    public Skipdoor Use;

    public CaravanArrivalAction_UseSkipdoor(Skipdoor origin, Skipdoor dest)
    {
        this.Use    = origin;
        this.Target = dest;
    }

    public override string Label => "VPE.TeleportTo".Translate(this.Target.Name);

    public override string ReportString =>
        JobUtility.GetResolvedJobReportRaw(VPE_DefOf.VPE_UseSkipdoor.reportString, this.Use.Name, this.Use, this.Target.Name, this.Target, null, null);

    public override void Arrived(Caravan caravan)
    {
        caravan.Tile = this.Target.Map.Tile;
        caravan.Notify_Teleported();
    }

    public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile) =>
        this.Target is { Spawned: true } && this.Use is { Spawned: true };

    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, Skipdoor origin, Skipdoor dest)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(
            () => true, () => new CaravanArrivalAction_UseSkipdoor(origin, dest),
            "VPE.TeleportTo".Translate(dest.Name), caravan, origin.Map.Tile, origin.Map.Parent);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref this.Target, "target");
        Scribe_References.Look(ref this.Use,    "use");
    }
}