namespace VanillaPsycastsExpanded.Skipmaster;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using Ability = VFECore.Abilities.Ability;

public class Ability_Chunkskip : Ability
{
    private readonly Dictionary<LocalTargetInfo, HashSet<Thing>> foundChunksCache = new();

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
        {
            AbilityExtension_Clamor clamor = this.def.GetModExtension<AbilityExtension_Clamor>();
            foreach (Thing thing in this.FindClosestChunks(target.HasThing ? new LocalTargetInfo(target.Thing) : new LocalTargetInfo(target.Cell)))
                if (this.FindFreeCell(target.Cell, this.pawn.Map, out IntVec3 intVec))
                {
                    AbilityUtility.DoClamor(thing.Position, clamor.clamorRadius, this.pawn, clamor.clamorType);
                    AbilityUtility.DoClamor(intVec,         clamor.clamorRadius, this.pawn, clamor.clamorType);
                    this.AddEffecterToMaintain(EffecterDefOf.Skip_Entry.Spawn(thing.Position, target.Map, 0.72f), thing.Position, 60);
                    this.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(intVec, target.Map, 0.72f),   intVec,         60);
                    FleckMaker.ThrowDustPuffThick(intVec.ToVector3(), target.Map, Rand.Range(1.5f, 3f), CompAbilityEffect_Chunkskip.DustColor);
                    thing.Position = intVec;
                }

            SoundDefOf.Psycast_Skip_Pulse.PlayOneShot(new TargetInfo(target.Cell, this.pawn.Map));
        }
    }

    public override void WarmupToil(Toil toil)
    {
        base.WarmupToil(toil);
        toil.AddPreTickAction(delegate
        {
            if (this.pawn.jobs.curDriver.ticksLeftThisToil == 5)
                foreach (Thing t2 in this.FindClosestChunks(this.pawn.jobs.curJob.targetA))
                    FleckMaker.Static(t2.TrueCenter(), this.pawn.Map, FleckDefOf.PsycastSkipFlashEntry, 0.72f);
        });
    }

    private IEnumerable<Thing> FindClosestChunks(LocalTargetInfo target)
    {
        if (this.foundChunksCache.TryGetValue(target, out HashSet<Thing> foundChunks)) return foundChunks;
        foundChunks = new HashSet<Thing>();
        RegionTraverser.BreadthFirstTraverse(target.Cell, this.pawn.Map, (from, to) => true, delegate(Region x)
        {
            List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Chunk);
            int         num  = 0;
            while (num < list.Count && foundChunks.Count < this.GetPowerForPawn())
            {
                Thing thing = list[num];
                if (!thing.Fogged() && !foundChunks.Contains(thing)) foundChunks.Add(thing);

                num++;
            }

            return foundChunks.Count >= this.GetPowerForPawn();
        }, 999999, RegionType.Set_All);
        this.foundChunksCache.Add(target, foundChunks);
        return foundChunks;
    }

    private bool FindFreeCell(IntVec3 target, Map map, out IntVec3 result)
    {
        return CellFinder.TryFindRandomCellNear(target, map, Mathf.RoundToInt(this.GetRadiusForPawn()) - 1,
                                                cell =>
                                                    CompAbilityEffect_WithDest.CanTeleportThingTo(cell, map) &&
                                                    GenSight.LineOfSight(cell, target, map, true), out result);
    }

    public override void DrawHighlight(LocalTargetInfo target)
    {
        base.DrawHighlight(target);
        foreach (Thing t in this.FindClosestChunks(target))
        {
            GenDraw.DrawLineBetween(t.TrueCenter(), target.CenterVector3);
            GenDraw.DrawTargetHighlight(t);
        }
    }

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (!target.Cell.Standable(this.pawn.Map)) return false;

        if (target.Cell.Filled(this.pawn.Map)) return false;

        if (!this.FindClosestChunks(target).Any())
        {
            if (showMessages) Messages.Message("VPE.NoChunks".Translate(), this.pawn, MessageTypeDefOf.RejectInput, false);
            return false;
        }

        if (!this.FindFreeCell(target.Cell, this.pawn.Map, out IntVec3 _))
        {
            if (showMessages) Messages.Message("AbilityNotEnoughFreeSpace".Translate(), this.pawn, MessageTypeDefOf.RejectInput, false);
            return false;
        }

        return base.ValidateTarget(target, showMessages);
    }
}

public class AbilityExtension_Clamor : DefModExtension
{
    public int       clamorRadius;
    public ClamorDef clamorType;
}