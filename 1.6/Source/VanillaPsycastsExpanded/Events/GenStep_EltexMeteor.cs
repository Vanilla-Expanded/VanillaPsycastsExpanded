namespace VanillaPsycastsExpanded
{
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class GenStep_EltexMeteor : GenStep_ScatterLumpsMineable
    {
        public override int SeedPart => 1634184421;

        public override void Generate(Map map, GenStepParams parms)
        {
            forcedDefToScatter = VPE_DefOf.VPE_EltexOre;
            count = 1;
            forcedLumpSize = 9;
            base.Generate(map, parms);
        }

        protected override bool CanScatterAt(IntVec3 c, Map map)
        {
            if (MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var var) && var.Any((CellRect x) => x.Contains(c)))
            {
                return false;
            }
            return map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors));
        }

        protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            base.ScatterAt(c, map, parms, stackCount);
            int minX = recentLumpCells.Min((IntVec3 x) => x.x);
            int minZ = recentLumpCells.Min((IntVec3 x) => x.z);
            int maxX = recentLumpCells.Max((IntVec3 x) => x.x);
            int maxZ = recentLumpCells.Max((IntVec3 x) => x.z);
            CellRect var = CellRect.FromLimits(minX, minZ, maxX, maxZ);
            MapGenerator.SetVar("RectOfInterest", var);
        }
    }
}