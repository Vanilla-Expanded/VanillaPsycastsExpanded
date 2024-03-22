namespace VanillaPsycastsExpanded.Wildspeaker
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using VFECore.Abilities;

    [HarmonyPatch]
    public abstract class PlantSpawner : TunnelHiveSpawner
    {
        private static readonly AccessTools.FieldRef<TunnelHiveSpawner, int> secondSpawnTickRef =
            AccessTools.FieldRefAccess<TunnelHiveSpawner, int>("secondarySpawnTick");

        private ThingDef plantDef;

        [HarmonyPatch(typeof(TunnelHiveSpawner), nameof(Tick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TickTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            MethodInfo            info  = AccessTools.Method(typeof(Rand), nameof(Rand.MTBEventOccurs));
            int                   idx1  = codes.FindIndex(ins => ins.opcode == OpCodes.Ldsfld);
            Label                 label = (Label) codes[codes.FindIndex(ins => ins.Calls(info)) + 1].operand;
            codes.InsertRange(idx1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Isinst, typeof(PlantSpawner)),
                new CodeInstruction(OpCodes.Brtrue, label)
            });
            return codes;
        }

        protected override void Spawn(Map map, IntVec3 loc)
        {
            if (this.plantDef == null) return;
            Plant plant = (Plant) GenSpawn.Spawn(this.plantDef, loc, map);
            this.SetupPlant(plant, loc, map);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                secondSpawnTickRef(this) = Find.TickManager.TicksGame + this.DurationTicks() + Rand.RangeInclusive(-60, 120);
                Rand.PushState(Find.TickManager.TicksGame);
                this.plantDef = this.ChoosePlant(this.Position, map);
                Rand.PopState();
            }

            if (!this.CheckSpawnLoc(this.Position, map) || this.plantDef == null) this.Destroy();
        }

        protected virtual bool CheckSpawnLoc(IntVec3 loc, Map map)
        {
            if (loc.GetTerrain(map).fertility == 0f) return false;
            List<Thing> list = loc.GetThingList(map);
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Thing thing = list[i];
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

        protected virtual void SetupPlant(Plant plant, IntVec3 loc, Map map)
        {
        }

        protected virtual int DurationTicks() => 3f.SecondsToTicks();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref this.plantDef, nameof(this.plantDef));
        }
    }

    public class BrambleSpawner : PlantSpawner
    {
        protected override void SetupPlant(Plant plant, IntVec3 loc, Map map)
        {
            base.SetupPlant(plant, loc, map);
            if (this.TryGetComp<CompDuration>() is {durationTicksLeft: var ticks})
                Current.Game.GetComponent<GameComponent_PsycastsManager>().removeAfterTicks.Add((plant, Find.TickManager.TicksGame + ticks));
        }

        protected override ThingDef ChoosePlant(IntVec3 loc, Map map) => VPE_DefOf.Plant_Brambles;
    }

    public class WildPlantSpawner : PlantSpawner
    {
        protected override ThingDef ChoosePlant(IntVec3 loc, Map map)
        {
            if (Rand.Chance(0.2f)) return null;
            if (DefDatabase<ThingDef>.AllDefs.Where(td => td.plant is {Sowable: true, IsTree: false}).TryRandomElement(out ThingDef plantDef))
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
            if (map.Biome.AllWildPlants.Where(td => td.plant is {IsTree: true}).TryRandomElement(out ThingDef plantDef) ||
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
}