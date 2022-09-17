namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using System.Linq;
    using Verse;
    using Verse.AI;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;

    public class AbilityExtension_SpawnTemperatureArea : AbilityExtension_AbilityMod
    {
        public float fixedTemperature;
        public FleckDef fleckToSpawnInArea;
        public float spawnRate;

        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            foreach (var target in targets)
            {
                ability.pawn.Map.GetComponent<MapComponent_PsycastsManager>().temperatureZones.Add(new FixedTemperatureZone
                {
                    fixedTemperature = fixedTemperature,
                    radius = ability.GetRadiusForPawn(),
                    center = target.Cell,
                    expiresIn = Find.TickManager.TicksGame + ability.GetDurationForPawn(),
                    fleckToSpawn = fleckToSpawnInArea,
                    spawnRate = spawnRate
                });
            }
        }
    }
}