namespace VanillaPsycastsExpanded.Harmonist;

using System.Collections.Generic;
using HarmonyLib;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

public class Ability_TransmuteStone : Ability
{
    private static readonly AccessTools.FieldRef<World, List<ThingDef>> allNaturalRockDefs =
        AccessTools.FieldRefAccess<World, List<ThingDef>>("allNaturalRockDefs");

    private static readonly AccessTools.FieldRef<Thing, Graphic> graphicInt = AccessTools.FieldRefAccess<Thing, Graphic>("graphicInt");

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
        {
            Map map = target.Map;
            Find.World.NaturalRockTypesIn(map.Tile); // Force the game to generate the rocks list we are querying
            List<ThingDef> naturalRockDefs = allNaturalRockDefs(Find.World);
            ThingDef       chosenRock      = naturalRockDefs.RandomElement();

            TerrainDef NewTerrain(TerrainDef terrain)
            {
                string name = terrain.defName;
                foreach (ThingDef rockDef in naturalRockDefs.Except(chosenRock))
                    if (name.StartsWith(rockDef.defName))
                        name = name.Replace(rockDef.defName, chosenRock.defName);
                return TerrainDef.Named(name);
            }

            void ClearCache(Thing thing)
            {
                graphicInt(thing) = null;
                thing.DirtyMapMesh(map);
            }

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(target.Cell, this.GetRadiusForPawn(), true))
            {
                foreach (Thing thing in cell.GetThingList(map))
                    if (thing.def.IsNonResourceNaturalRock)
                    {
                        thing.def = chosenRock;
                        ClearCache(thing);
                    }
                    else
                        foreach (ThingDef rockDef in naturalRockDefs)
                            if (rockDef.building.mineableThing == thing.def)
                            {
                                thing.def = chosenRock.building.mineableThing;
                                ClearCache(thing);
                            }
                            else if (rockDef.building.smoothedThing == thing.def)
                            {
                                thing.def = chosenRock.building.smoothedThing;
                                ClearCache(thing);
                            }
                            else if (rockDef.building.mineableThing.butcherProducts[0].thingDef == thing.def)
                            {
                                thing.def = chosenRock.building.mineableThing.butcherProducts[0].thingDef;
                                ClearCache(thing);
                            }
                            else if (thing.Stuff != null && thing.Stuff == rockDef.building.mineableThing.butcherProducts[0].thingDef)
                            {
                                thing.SetStuffDirect(chosenRock.building.mineableThing.butcherProducts[0].thingDef);
                                ClearCache(thing);
                            }

                TerrainGrid grid    = map.terrainGrid;
                TerrainDef  terrain = grid.TerrainAt(cell);
                grid.SetTerrain(cell, NewTerrain(terrain));
                terrain = grid.UnderTerrainAt(cell);
                if (terrain != null) grid.SetUnderTerrain(cell, NewTerrain(terrain));
            }
        }
    }
}