using System.Collections.Generic;
using HarmonyLib;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Harmonist;

public class Ability_TransmuteStone : Ability
{
    private static readonly AccessTools.FieldRef<World, List<ThingDef>> allNaturalRockDefs =
        AccessTools.FieldRefAccess<World, List<ThingDef>>("allNaturalRockDefs");

    private static readonly AccessTools.FieldRef<Thing, Graphic> graphicInt = AccessTools.FieldRefAccess<Thing, Graphic>("graphicInt");

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (var target in targets)
        {
            var map = target.Map;
            Find.World.NaturalRockTypesIn(map.Tile); // Force the game to generate the rocks list we are querying
            var naturalRockDefs = allNaturalRockDefs(Find.World);
            var chosenRock = naturalRockDefs.RandomElement();

            TerrainDef NewTerrain(TerrainDef terrain)
            {
                var name = terrain.defName;
                foreach (var rockDef in naturalRockDefs.Except(chosenRock))
                    if (name.StartsWith(rockDef.defName))
                        name = name.Replace(rockDef.defName, chosenRock.defName);
                return TerrainDef.Named(name);
            }

            void Replace(Thing thing, ThingDef def = null, ThingDef stuff = null)
            {
                var owner = thing.holdingOwner;
                var cell = thing.Position;
                var rot = thing.Rotation;
                def ??= thing.def;
                stuff ??= thing.Stuff;
                var newThing = ThingMaker.MakeThing(def, stuff);
                var des = map.designationManager.AllDesignationsOn(thing).ListFullCopy();
                thing.Destroy();
                if (cell.IsValid)
                    GenSpawn.Spawn(newThing, cell, map, rot);
                else if (owner != null)
                {
                    if (!owner.TryAdd(newThing)) Log.Error($"[VPE] Failed to add {newThing} to {owner}");
                }
                else
                {
                    Log.Warning($"[VPE] Attempting to replace unspawned and unheld thing {thing}");
                    return;
                }

                foreach (var designation in des) map.designationManager.AddDesignation(new Designation(newThing, designation.def));
            }

            foreach (var cell in GenRadial.RadialCellsAround(target.Cell, GetRadiusForPawn(), true))
            {
                foreach (var thing in cell.GetThingList(map).ListFullCopy())
                    if (thing.def.IsNonResourceNaturalRock)
                        Replace(thing, chosenRock);
                    else
                        foreach (var rockDef in naturalRockDefs)
                            if (rockDef.building.mineableThing == thing.def)
                                Replace(thing, chosenRock.building.mineableThing);
                            else if (rockDef.building.smoothedThing == thing.def)
                                Replace(thing, chosenRock.building.smoothedThing);
                            else if (rockDef.building.mineableThing.butcherProducts[0].thingDef == thing.def)
                                Replace(thing, chosenRock.building.mineableThing.butcherProducts[0].thingDef);
                            else if (thing.Stuff != null && thing.Stuff == rockDef.building.mineableThing.butcherProducts[0].thingDef)
                                Replace(thing, stuff: chosenRock.building.mineableThing.butcherProducts[0].thingDef);

                var grid = map.terrainGrid;
                var terrain = grid.TerrainAt(cell);
                grid.SetTerrain(cell, NewTerrain(terrain));
                terrain = grid.UnderTerrainAt(cell);
                if (terrain != null) grid.SetUnderTerrain(cell, NewTerrain(terrain));
            }
        }
    }
}