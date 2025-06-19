using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace VanillaPsycastsExpanded.Nightstalker;

[HarmonyPatch]
[StaticConstructorOnStartup]
public class CompDarkener : ThingComp
{
    private static readonly Dictionary<Map, Dictionary<IntVec3, int>> darkCells = new();

    private CompProperties_Darkness Props => (CompProperties_Darkness)props;

    private static Dictionary<IntVec3, int> DarkCellsFor(Map map, bool create = true)
    {
        if (!darkCells.TryGetValue(map, out var cells))
        {
            if (!create)
                return null;

            cells = new Dictionary<IntVec3, int>();
            darkCells[map] = cells;
        }

        return cells;
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        var cells = DarkCellsFor(parent.Map);

        foreach (var pos in GenRadial.RadialCellsAround(parent.Position, Props.darknessRange, true))
        {
            if (cells.TryGetValue(pos, out var darkeners))
                cells[pos] = darkeners + 1;
            else
                cells[pos] = 1;

            parent.Map.glowGrid.LightBlockerAdded(pos);
        }
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        var cells = DarkCellsFor(map);

        foreach (var pos in GenRadial.RadialCellsAround(parent.Position, Props.darknessRange, true))
        {
            if (cells.TryGetValue(pos, out var darkeners))
            {
                if (darkeners == 1)
                    cells.Remove(pos);
                else
                    cells[pos] = darkeners - 1;
            }
            else darkeners = 0;

            // There may be some other building blocking light in the cell already,
            // so we can only
            var anyBlockers = false;
            var list = map.thingGrid.ThingsListAt(pos);
            for (var i = 0; i < list.Count; i++)
            {
                if (darkeners > 0 || IsLightBlocker(list[i]))
                {
                    anyBlockers = true;
                    break;
                }
            }

            if (!anyBlockers)
                parent.Map.glowGrid.LightBlockerRemoved(pos);
        }

        // Do a bit of cleanup
        if (!cells.Any())
            darkCells.Remove(map);
    }

    // In vanilla, blockLight only works with buildings.
    // Also, CompDarkener is handled differently so no need to check for it.
    private static bool IsLightBlocker(Thing thing) => thing.def.blockLight && thing is Building;

    [HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.GroundGlowAt))]
    [HarmonyPrefix]
    public static void IgnoreSkyDark(IntVec3 c, ref bool ignoreSky, Map ___map)
    {
        if (darkCells.TryGetValue(___map, out var set) && set.ContainsKey(c)) ignoreSky = true;
    }
}

public class CompProperties_Darkness : CompProperties
{
    public float darknessRange;

    public CompProperties_Darkness() => compClass = typeof(CompDarkener);
}
