namespace VanillaPsycastsExpanded.Skipmaster;

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
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

    public override void DoTeleportEffects(Thing thing, int ticksLeftThisToil, Map targetMap, ref IntVec3 targetCell, DoorTeleporter dest)
    {
        if (ticksLeftThisToil == 5)
        {
            FleckMaker.Static(thing.Position, thing.Map, FleckDefOf.PsycastSkipFlashEntry);
            FleckMaker.Static(targetCell, targetMap, FleckDefOf.PsycastSkipInnerExit);
            FleckMaker.Static(targetCell, targetMap, FleckDefOf.PsycastSkipOuterRingExit);
            SoundDefOf.Psycast_Skip_Entry.PlayOneShot(this);
            SoundDefOf.Psycast_Skip_Exit.PlayOneShot(dest);
        }
        else if (ticksLeftThisToil == 15)
        {
            targetCell = GenAdj.CellsAdjacentCardinal(dest).Where(c => c.Standable(targetMap)).RandomElement();
            teleportEffecters[thing] = EffecterDefOf.Skip_Exit.Spawn(targetCell, targetMap);
            teleportEffecters[thing].ticksLeft = 15;
        }

        if (teleportEffecters.ContainsKey(thing))
        {
            teleportEffecters[thing].EffectTick(new TargetInfo(targetCell, targetMap), new TargetInfo(targetCell, targetMap));
        }
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