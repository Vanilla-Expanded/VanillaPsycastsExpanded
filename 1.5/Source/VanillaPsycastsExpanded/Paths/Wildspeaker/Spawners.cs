using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Wildspeaker;

[HarmonyPatch]
public abstract class PlantSpawner : GroundSpawner
{
    private ThingDef plantDef;

    protected override void Spawn(Map map, IntVec3 loc)
    {
        if (plantDef == null) return;
        var plant = (Plant)GenSpawn.Spawn(plantDef, loc, map);
        SetupPlant(plant, loc, map);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (!respawningAfterLoad)
        {
            secondarySpawnTick = Find.TickManager.TicksGame + DurationTicks() + Rand.RangeInclusive(-60, 120);
            filthSpawnMTB = float.PositiveInfinity;
            Rand.PushState(Find.TickManager.TicksGame);
            plantDef = ChoosePlant(Position, map);
            Rand.PopState();
        }

        if (!CheckSpawnLoc(Position, map) || plantDef == null) Destroy();
    }

    protected virtual bool CheckSpawnLoc(IntVec3 loc, Map map)
    {
        if (loc.GetTerrain(map).fertility == 0f) return false;
        var list = loc.GetThingList(map);
        for (var i = list.Count - 1; i >= 0; i--)
        {
            var thing = list[i];
            if (thing is Plant)
            {
                if (thing.def.plant.IsTree) return false;
                thing.Destroy();
            }

            if (thing.def.IsEdifice()) return false;
        }

        return true;
    }

    protected abstract ThingDef ChoosePlant(IntVec3 loc, Map map);

    protected virtual void SetupPlant(Plant plant, IntVec3 loc, Map map) { }

    protected virtual int DurationTicks() => 3f.SecondsToTicks();

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref plantDef, nameof(plantDef));
    }
}

public class BrambleSpawner : PlantSpawner
{
    protected override void SetupPlant(Plant plant, IntVec3 loc, Map map)
    {
        base.SetupPlant(plant, loc, map);
        if (this.TryGetComp<CompDuration>() is { durationTicksLeft: var ticks })
            Current.Game.GetComponent<GameComponent_PsycastsManager>().removeAfterTicks.Add((plant, Find.TickManager.TicksGame + ticks));
    }

    protected override ThingDef ChoosePlant(IntVec3 loc, Map map) => VPE_DefOf.Plant_Brambles;
}

public class WildPlantSpawner : PlantSpawner
{
    protected override ThingDef ChoosePlant(IntVec3 loc, Map map)
    {
        if (Rand.Chance(0.2f)) return null;
        if (DefDatabase<ThingDef>.AllDefs.Where(td => td.plant is { Sowable: true, IsTree: false }).TryRandomElement(out var plantDef))
            if (plantDef.CanEverPlantAt(loc, map, true))
                return plantDef;

        return null;
    }

    protected override void SetupPlant(Plant plant, IntVec3 loc, Map map)
    {
        base.SetupPlant(plant, loc, map);
        plant.Growth = Mathf.Clamp(this.TryGetComp<CompAbilitySpawn>().pawn.GetStatValue(StatDefOf.PsychicSensitivity) - 1f, 0.1f, 1f);
    }
}

public class TreeSpawner : PlantSpawner
{
    protected override int DurationTicks() => 5f.SecondsToTicks();

    protected override ThingDef ChoosePlant(IntVec3 loc, Map map)
    {
        if (map.Biome.AllWildPlants.Where(td => td.plant is { IsTree: true }).TryRandomElement(out var plantDef) ||
            map.Biome.AllWildPlants.TryRandomElement(out plantDef))
            if (plantDef.CanEverPlantAt(loc, map, true) && PlantUtility.AdjacentSowBlocker(plantDef, loc, map) == null)
                return plantDef;
        return null;
    }

    protected override void SetupPlant(Plant plant, IntVec3 loc, Map map)
    {
        if (PlantUtility.AdjacentSowBlocker(plant.def, loc, map) != null) plant.Destroy();
        else plant.Growth = 1f;
    }
}
