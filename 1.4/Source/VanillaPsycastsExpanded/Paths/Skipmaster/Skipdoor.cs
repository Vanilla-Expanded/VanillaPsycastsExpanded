namespace VanillaPsycastsExpanded.Skipmaster;

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;
using VFECore;
[StaticConstructorOnStartup]
public class Skipdoor : DoorTeleporter, IMinHeatGiver
{
    public Pawn Pawn;
    public bool IsActive => this.Spawned;
    public int MinHeat => 50;
    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        this.Pawn.Psycasts().AddMinHeatGiver(this);
        if (respawningAfterLoad) return;
        this.Pawn.psychicEntropy.TryAddEntropy(50f, this, true, true);
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref this.Pawn, "pawn");
    }
    public override void Tick()
    {
        base.Tick();
        if (this.IsHashIntervalTick(30) && this.HitPoints < this.MaxHitPoints) this.HitPoints += 1;
    }

    public override void DoTeleportEffects(JobDriver_UseDoorTeleporter jobDriver)
    {
        Map targetMap = jobDriver.job.globalTarget.Map;
        if (jobDriver.ticksLeftThisToil == 5)
        {
            FleckMaker.Static(jobDriver.pawn.Position, jobDriver.pawn.Map, FleckDefOf.PsycastSkipFlashEntry);
            FleckMaker.Static(jobDriver.targetCell, targetMap, FleckDefOf.PsycastSkipInnerExit);
            FleckMaker.Static(jobDriver.targetCell, targetMap, FleckDefOf.PsycastSkipOuterRingExit);
            SoundDefOf.Psycast_Skip_Entry.PlayOneShot(jobDriver.Origin);
            SoundDefOf.Psycast_Skip_Exit.PlayOneShot(jobDriver.Dest);
        }
        else if (jobDriver.ticksLeftThisToil == 15)
        {
            jobDriver.targetCell = GenAdj.CellsAdjacentCardinal(jobDriver.Dest).Where(c => c.Standable(targetMap)).RandomElement();
            jobDriver.destEffecter = EffecterDefOf.Skip_Exit.Spawn(jobDriver.targetCell, targetMap);
            jobDriver.destEffecter.ticksLeft = 15;
        }
        jobDriver.destEffecter?.EffectTick(new TargetInfo(jobDriver.targetCell, targetMap), new TargetInfo(jobDriver.targetCell, targetMap));
    }
    public override IEnumerable<Gizmo> GetDoorTeleporterGismoz()
    {
        var extension = def.GetModExtension<DoorTeleporterExtension>();
        var doorMaterials = doorTeleporterMaterials[def];
        if (doorMaterials.DestroyIcon != null)
        {
            yield return new Command_Action
            {
                defaultLabel = extension.destroyLabelKey.Translate(),
                defaultDesc = extension.destroyDescKey.Translate(this.Pawn.NameFullColored),
                icon = doorMaterials.DestroyIcon,
                action = () => this.Destroy()
            };
        }
        if (doorMaterials.RenameIcon != null)
        {
            yield return new Command_Action
            {
                defaultLabel = extension.renameLabelKey.Translate(),
                defaultDesc = extension.renameDescKey.Translate(),
                icon = doorMaterials.RenameIcon,
                action = () => Find.WindowStack.Add(new Dialog_RenameDoorTeleporter(this))
            };
        }
    }

    protected override void PlaySustainer(SoundDef sustainer)
    {
        if (PsycastsMod.Settings.muteSkipdoor)
        {
            this.sustainer?.End();
            this.sustainer = null;
        }
        else
        {
            this.sustainer ??= sustainer.TrySpawnSustainer(this);
            this.sustainer.Maintain();
        }
    }
}