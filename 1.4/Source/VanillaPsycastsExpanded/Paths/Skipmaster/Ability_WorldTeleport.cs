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

public class Ability_WorldTeleport : Ability
{
    public override void DoAction()
    {
        Pawn pawn = this.PawnsToSkip().FirstOrDefault(p => p.IsQuestLodger());
        if (pawn != null) Dialog_MessageBox.CreateConfirmation("FarskipConfirmTeleportingLodger".Translate(pawn.Named("PAWN")), base.DoAction);
        else base.DoAction();
    }

    private IEnumerable<Pawn> PawnsToSkip()
    {
        Caravan caravan = this.pawn.GetCaravan();
        if (caravan != null)
            foreach (Pawn pawn in caravan.pawns)
                yield return pawn;
        else
        {
            bool homeMap = this.pawn.Map.IsPlayerHome;
            foreach (Thing thing in GenRadial
                         .RadialDistinctThingsAround(this.pawn.Position, this.pawn.Map, this.GetRadiusForPawn(), true))
                if (thing is Pawn { Dead: false } pawn2 && (pawn2.IsColonist || pawn2.IsPrisonerOfColony ||
                                                            (!homeMap && pawn2.RaceProps.Animal && pawn2.Faction is { IsPlayer: true })))
                    yield return pawn2;
        }
    }

    private Pawn AlliedPawnOnMap(Map targetMap)
    {
        return targetMap.mapPawns.AllPawnsSpawned.FirstOrDefault(p => !p.NonHumanlikeOrWildMan()        && p.IsColonist &&
                                                                      p.HomeFaction == Faction.OfPlayer && !this.PawnsToSkip().Contains(p));
    }

    private bool ShouldEnterMap(GlobalTargetInfo target)
    {
        if (target.WorldObject is Caravan caravan && caravan.Faction == this.pawn.Faction) return false;

        return target.WorldObject is MapParent { HasMap: true } mapParent && (this.AlliedPawnOnMap(mapParent.Map) != null || mapParent.Map == this.pawn.Map);
    }

    public override bool CanHitTargetTile(GlobalTargetInfo target)
    {
        Caravan caravan = this.pawn.GetCaravan();
        if (caravan is { ImmobilizedByMass: true }) return false;
        Caravan caravan1 = target.WorldObject as Caravan;

        return (caravan == null || caravan != caravan1) && (this.ShouldEnterMap(target) || (caravan1 != null && caravan1.Faction == this.pawn.Faction)) &&
               base.CanHitTargetTile(target);
    }

    public override bool IsEnabledForPawn(out string reason)
    {
        if (!base.IsEnabledForPawn(out reason)) return false;

        if (this.pawn.GetCaravan() is { ImmobilizedByMass: true })
        {
            reason = "CaravanImmobilizedByMass".Translate();
            return false;
        }

        return true;
    }

    public override void WarmupToil(Toil toil)
    {
        base.WarmupToil(toil);
        toil.AddPreTickAction(delegate
        {
            if (this.pawn.jobs.curDriver.ticksLeftThisToil != 5) return;
            foreach (Pawn p in this.PawnsToSkip())
            {
                FleckCreationData dataAttachedOverlay = FleckMaker.GetDataAttachedOverlay(p, FleckDefOf.PsycastSkipFlashEntry, Vector3.zero);
                dataAttachedOverlay.link.detachAfterTicks = 5;
                p.Map.flecks.CreateFleck(dataAttachedOverlay);
                this.AddEffecterToMaintain(EffecterDefOf.Skip_Entry.Spawn(this.pawn, this.pawn.Map), this.pawn.Position, 60);
            }
        });
    }

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        Caravan caravan = this.pawn.GetCaravan();
        Map targetMap = targets[0].WorldObject is MapParent mapParent ? mapParent.Map : null;
        IntVec3 targetCell = IntVec3.Invalid;
        List<Pawn> list = this.PawnsToSkip().ToList();
        if (this.pawn.Spawned) SoundDefOf.Psycast_Skip_Pulse.PlayOneShot(new TargetInfo(targets[0].Cell, this.pawn.Map));
        if (targetMap != null && this.AlliedPawnOnMap(targetMap) is { Position: var alliedPawnCell }) targetCell = alliedPawnCell;
        AbilityExtension_Clamor clamor = this.def.GetModExtension<AbilityExtension_Clamor>();
        if (targetCell.IsValid)
        {
            foreach (Pawn pawn3 in list)
            {
                if (pawn3.Spawned)
                {
                    pawn3.teleporting = true;
                    pawn3.ExitMap(false, Rot4.Invalid);
                    AbilityUtility.DoClamor(pawn3.Position, clamor.clamorRadius, this.pawn, clamor.clamorType);
                    pawn3.teleporting = false;
                }

                CellFinder.TryFindRandomSpawnCellForPawnNear(targetCell, targetMap, out IntVec3 intVec, 4, cell =>
                                                                 cell != targetCell && cell.GetRoom(targetMap) == targetCell.GetRoom(targetMap));
                GenSpawn.Spawn(pawn3, intVec, targetMap);
                if (pawn3.drafter != null && pawn3.IsColonistPlayerControlled) pawn3.drafter.Drafted = true;
                pawn3.Notify_Teleported();
                if (pawn3.IsPrisoner) pawn3.guest.WaitInsteadOfEscapingForDefaultTicks();
                this.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(pawn3, pawn3.Map), pawn3.Position, 60, targetMap);
                SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(intVec, pawn3.Map));
                if ((pawn3.IsColonist || pawn3.RaceProps.packAnimal) && pawn3.Map.IsPlayerHome) pawn3.inventory.UnloadEverything = true;
            }

            if (Find.WorldSelector.IsSelected(caravan))
            {
                Find.WorldSelector.Deselect(caravan);
                CameraJumper.TryJump(targetCell, targetMap);
            }

            caravan?.Destroy();
        }
        else if (targets[0].WorldObject is Caravan caravan2 && caravan2.Faction == this.pawn.Faction)
        {
            if (caravan != null)
            {
                caravan.pawns.TryTransferAllToContainer(caravan2.pawns);
                caravan2.Notify_Merged(new List<Caravan>
                {
                    caravan
                });
                caravan.Destroy();
            }
            else
                foreach (Pawn pawn4 in list)
                {
                    caravan2.AddPawn(pawn4, true);
                    pawn4.ExitMap(false, Rot4.Invalid);
                    AbilityUtility.DoClamor(pawn4.Position, clamor.clamorRadius, this.pawn, clamor.clamorType);
                }
        }
        else if (caravan != null)
        {
            caravan.Tile = targets[0].Tile;
            caravan.pather.StopDead();
        }
        else
        {
            CaravanMaker.MakeCaravan(list, this.pawn.Faction, targets[0].Tile, false);
            foreach (Pawn pawn5 in list) pawn5.ExitMap(false, Rot4.Invalid);
        }


        base.Cast(targets);
    }

    public override void GizmoUpdateOnMouseover()
    {
        base.GizmoUpdateOnMouseover();
        GenDraw.DrawRadiusRing(this.pawn.Position, this.GetRadiusForPawn(), Color.blue);
    }
}