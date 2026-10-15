using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VEF.Abilities.Ability;

namespace VanillaPsycastsExpanded.Wildspeaker;

public class Ability_SummonPack : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        var map = targets[0].Map;
        var points = GetPowerForPawn();
        List<Pawn> animals = new();

        while (points > 0 && AggressiveAnimalIncidentUtility.TryFindAggressiveAnimalKind(points, map.Tile, out var kind))
        {
            points -= kind.combatPower;
            var animal = PawnGenerator.GeneratePawn(new(kind, tile: map.Tile));
            animals.Add(animal);
        }

        if (!RCellFinder.TryFindRandomPawnEntryCell(out var entryCell, map, CellFinder.EdgeRoadChance_Animal))
            entryCell = CellFinder.RandomEdgeCell(map);

        for (var i = 0; i < animals.Count; i++)
        {
            var animal = animals[i];
            GenSpawn.Spawn(animal, CellFinder.RandomClosewalkCellNear(entryCell, map, 10), map);
            animal.mindState.mentalStateHandler.TryStartMentalState(VPE_DefOf.VPE_ManhunterTerritorial);
            animal.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(25000, 35000);
        }

        Find.LetterStack.ReceiveLetter("VPE.PackSummon".Translate(), "VPE.PackSummon.Desc".Translate(pawn.NameShortColored), LetterDefOf.PositiveEvent,
            new TargetInfo(entryCell, map));
    }
}
