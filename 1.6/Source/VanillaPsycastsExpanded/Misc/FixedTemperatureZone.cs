namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class FixedTemperatureZone : IExposable
    {
        public IntVec3 center;

        public float radius;

        public int expiresIn;

        public float fixedTemperature;

        public FleckDef fleckToSpawn;

        public float spawnRate;
        public void DoEffects(Map map)
        {
            foreach (var cell in GenRadial.RadialCellsAround(center, radius, true))
            {
                if (Rand.Value < spawnRate)
                {
                    ThrowFleck(cell, map, 2.3f);
                    if (fixedTemperature < 0f)
                    {
                        map.snowGrid.AddDepth(cell, 0.1f);
                    }
                }
            }
        }
        public void ThrowFleck(IntVec3 c, Map map, float size)
        {
            Vector3 vector = c.ToVector3Shifted();
            if (vector.ShouldSpawnMotesAt(map))
            {
                vector += size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
                if (vector.InBounds(map))
                {
                    FleckCreationData dataStatic = FleckMaker.GetDataStatic(vector, map, fleckToSpawn, Rand.Range(4f, 6f) * size);
                    dataStatic.rotationRate = Rand.Range(-3f, 3f);
                    dataStatic.velocityAngle = Rand.Range(0, 360);
                    dataStatic.velocitySpeed = 0.12f;
                    map.flecks.CreateFleck(dataStatic);
                }
            }
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref center, "center");
            Scribe_Values.Look(ref radius, "radius");
            Scribe_Values.Look(ref expiresIn, "expiresIn");
            Scribe_Values.Look(ref fixedTemperature, "fixedTemperature");
            Scribe_Values.Look(ref spawnRate, "spawnRate");
            Scribe_Defs.Look(ref fleckToSpawn, "fleckSpawn");
        }
    }
}