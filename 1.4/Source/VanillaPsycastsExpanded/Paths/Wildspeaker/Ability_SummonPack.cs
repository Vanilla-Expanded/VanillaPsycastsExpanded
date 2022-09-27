namespace VanillaPsycastsExpanded.Wildspeaker
{
    using System.Collections.Generic;
    using RimWorld;
    using RimWorld.Planet;
    using Verse;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_SummonPack : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            Map        map     = targets[0].Map;
            float      points  = this.GetPowerForPawn();
            List<Pawn> animals = new();

            while (points > 0 && ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(points, map.Tile, out PawnKindDef kind))
            {
                points -= kind.combatPower;
                Pawn animal = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, tile: map.Tile));
                animals.Add(animal);
            }

            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 entryCell, map, CellFinder.EdgeRoadChance_Animal))
                entryCell = CellFinder.RandomEdgeCell(map);

            for (int i = 0; i < animals.Count; i++)
            {
                Pawn animal = animals[i];
                GenSpawn.Spawn(animal, CellFinder.RandomClosewalkCellNear(entryCell, map, 10), map);
                animal.mindState.mentalStateHandler.TryStartMentalState(VPE_DefOf.VPE_ManhunterTerritorial);
                animal.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(25000, 35000);
            }

            Find.LetterStack.ReceiveLetter("VPE.PackSummon".Translate(), "VPE.PackSummon.Desc".Translate(this.pawn.NameShortColored), LetterDefOf.PositiveEvent,
                                           new TargetInfo(entryCell, map));
        }
    }
}